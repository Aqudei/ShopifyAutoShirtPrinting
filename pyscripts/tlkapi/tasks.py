import json
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


@shared_task
def reset_database_task():
    """
    docstring
    """
    Log.objects.all().delete()
    LineItem.objects.all().delete()
    OrderInfo.objects.all().delete()
    Bin.objects.all().delete()


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
    exchange_name = 'thelonelykids'
    creds = pika.PlainCredentials('warwick', 'warwickpass1')
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='170.64.158.123', credentials=creds))
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
    exchange_name = 'thelonelykids'
    creds = pika.PlainCredentials('warwick', 'warwickpass1')
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='170.64.158.123', credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    message = json.dumps(ids)
    channel.basic_publish(exchange=exchange_name,
                          routing_key='items.added', body=message)
    connection.close()
