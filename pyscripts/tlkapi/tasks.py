import json
from uuid import uuid4
from celery import shared_task
import shopify
from django.conf import settings
from .models import (
    OrderInfo,
    LineItem,
    Log,
    Bin
)
import pikasender
import pika
from tlkapi.myshopify import find_order

@shared_task
def reset_database_task():
    """
    docstring
    """
    Log.objects.all().delete()
    LineItem.objects.all().delete()
    OrderInfo.objects.all().delete()
    Bin.objects.all().delete()


    if settings.BROADCAST_ENABLED:
        broadcast("database.reset","database.reset")


def process_orders(orders_response):
    """
    docstring
    """
    to_add = []
    for order in orders_response:
        created_order, _ = OrderInfo.objects.get_or_create(
            OrderId=order.id, OrderNumber=order.order_number)

        shipping_line = order.shipping_lines[0].code

        for line in order.line_items:
            if LineItem.objects.filter(LineItemId=line.id).exists():
                continue

            LineItem.objects.create(
                OrderNumber=order.order_number,
                Sku=line.sku,
                Name=line.name,
                VariantId=line.variant_id,
                VariantTitle=line.variant_title,
                LineItemId=line.id,
                Quantity=line.quantity,
                FulfillmentStatus=line.fulfillment_status or '',
                FinancialStatus=order.financial_status or '',
                Customer=f"{order.customer.first_name} {order.customer.last_name}",
                CustomerEmail=order.customer.email,
                Notes=order.note or '',
                OrderId=order.id,
                Status="Pending",
                Shipping=shipping_line,
                Order=created_order
            )


@shared_task
def fetch_orders():
    """
    docstring
    """
    session = shopify.Session(
        settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
    shopify.ShopifyResource.activate_session(session)

    orders_response = shopify.Order.find(
        fulfillment_status='unfulfilled', financial_status='paid')

    process_orders(orders_response)

    while (orders_response.has_next_page()):
        orders_response = orders_response.next_page()
        process_orders(orders_response)


@shared_task
def broadcast_updated(ids: list[int]):
    """
    docstring
    """
    exchange_name = settings.BROADCAST_EXCHANGE
    creds = pika.PlainCredentials(settings.BROADCAST_USERNAME, settings.BROADCAST_PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=settings.BROADCAST_HOST, credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    message = json.dumps(ids)
    channel.basic_publish(exchange=exchange_name,
                          routing_key='items.updated', body=message)
    connection.close()


@shared_task
def broadcast_added(ids: list[int]):
    """
    docstring
    """
    exchange_name = settings.BROADCAST_EXCHANGE
    creds = pika.PlainCredentials(settings.BROADCAST_USERNAME, settings.BROADCAST_PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=settings.BROADCAST_HOST, credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    message = json.dumps(ids)
    channel.basic_publish(exchange=exchange_name,
                          routing_key='items.added', body=message)
    connection.close()
    

@shared_task
def populate_info(line_pk):
    """
    docstring
    """
    line_item = LineItem.objects.get(Id=line_pk)
    order_number = line_item.OrderNumber

    if order_number in ['',None]:
        new_order_number = str(uuid4()).split("-")[0]
        order_info = OrderInfo.objects.create(OrderNumber=new_order_number)
    else:
        order_info_queryset = OrderInfo.objects.filter(OrderNumber=order_number)
        if order_info_queryset.exists():
            order_info  = order_info_queryset.first()
            sample = order_info.LineItems.first()            
            line_item.Customer = sample.Customer
            line_item.CustomerEmail = sample.CustomerEmail
        else:
            order_data = find_order(order_number)
            order_info  = OrderInfo.objects.create(
                OrderId = order_data.id,
                OrderNumber = order_number
            )
            line_item.Customer=f"{order_data.customer.first_name} {order_data.customer.last_name}",
            line_item.CustomerEmail=order_data.customer.email,

    order_info.save()
    
    line_item.Order = order_info
    line_item.save()

    if settings.BROADCAST_ENABLED:
        broadcast([line_item.Id],"items.updated")


@shared_task
def broadcast(message, routing_key):
    """
    docstring
    """
    exchange_name = settings.BROADCAST_EXCHANGE
    creds = pika.PlainCredentials(settings.BROADCAST_USERNAME, settings.BROADCAST_PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=settings.BROADCAST_HOST, credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    channel.basic_publish(exchange=exchange_name,
                          routing_key=routing_key, body=json.dumps(message))
    connection.close()
