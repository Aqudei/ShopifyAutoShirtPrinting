import json
from uuid import uuid4
from celery import shared_task
import shopify
from django.conf import settings
from django.db.models import F
from django.utils import timezone

from tools.models import Variant

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
    print("Clearing Logs...")
    Log.objects.all().delete()
    
    print("Clearing LineItems...")
    LineItem.objects.all().delete()

    
    print("Clearing Orders...")
    OrderInfo.objects.all().delete()

    
    print("Clearing Bins...")
    Bin.objects.all().delete()


    print("Regenerating Bins...")
    for i in range(settings.MAX_BINS):
        Bin.objects.get_or_create(Number=i)

    if settings.BROADCAST_ENABLED:
        broadcast("database.reset", "database.reset")


def process_orders(orders_response):
    """
    docstring
    """
    to_add = []
    for order in orders_response:
        created_order, _ = OrderInfo.objects.get_or_create(
            OrderId=order.id, OrderNumber=order.order_number)

        shipping_line = ''
        if order.shipping_lines and len(order.shipping_lines) > 0:
            shipping_line = order.shipping_lines[0].code

        for line in order.line_items:
            if LineItem.objects.filter(LineItemId=line.id).exists():
                continue
            
            variant = Variant.objects.filter(shopify_id=line.variant_id).first()
            image_src = ''
            if variant:
                image_src = variant.image_src
                
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
                Order=created_order,
                DateModified = timezone.now(),
                ProductImage = image_src
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
    creds = pika.PlainCredentials(
        settings.BROADCAST_USERNAME, settings.BROADCAST_PASSWORD)
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
    creds = pika.PlainCredentials(
        settings.BROADCAST_USERNAME, settings.BROADCAST_PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=settings.BROADCAST_HOST, credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    message = json.dumps(ids)
    channel.basic_publish(exchange=exchange_name,
                          routing_key='items.added', body=message)
    connection.close()


def clean_email(email: str):
    """
    docstring
    """
    if email in [None, '']:
        return email

    return email.strip(" \r\n\t().,")


@shared_task
def populate_info(line_pk):
    """
    docstring
    """

    line_item = LineItem.objects.get(Id=line_pk)
    order_number = line_item.OrderNumber

    if order_number in ['', None]:
        new_order_number = f"{OrderInfo.objects.new_order_number():0{8}}"
        order_info = OrderInfo.objects.create(OrderNumber=new_order_number)
        line_item.OrderNumber = new_order_number
    else:
        order_info_queryset = OrderInfo.objects.filter(
            OrderNumber=order_number)
        if order_info_queryset.exists():
            order_info = order_info_queryset.first()
            sample = order_info.LineItems.first()
            line_item.Customer = sample.Customer
            line_item.CustomerEmail = sample.CustomerEmail
        else:
            order_data = find_order(order_number)
            shipping_line = order_data.shipping_lines[0].code

            order_info = OrderInfo.objects.create(
                OrderId=order_data.id,
                OrderNumber=order_number
            )
            line_item.Customer = f"{order_data.customer.first_name} {order_data.customer.last_name}"
            line_item.CustomerEmail = order_data.customer.email
            line_item.OrderId = order_data.id
            line_item.Shipping = shipping_line

    order_info.save()

    line_item.Order = order_info
    line_item.save()

    if settings.BROADCAST_ENABLED:
        broadcast.delay([line_item.Id], "items.updated")


@shared_task
def archive_bin_task(BinNumber):
    bin = Bin.objects.get(Number=BinNumber)
    bin.Active = False
    bin.Notes = ''
    bin.save()

    orders = OrderInfo.objects.filter(Bin=bin)
    archived_items = set()
    for order in orders:
        order.Bin = None
        order.OriginalBinNumber = bin.Number
        order.save()

        LineItem.objects.filter(Order=order).update(
            Status='Archived'
        )

        for line_item_pk in LineItem.objects.filter(Order=order).values_list('Id', flat=True):
            archived_items.add(line_item_pk)

    if settings.BROADCAST_ENABLED:
        broadcast([bin.Number], "bins.destroyed")
        broadcast(archived_items, "items.archived")


@shared_task
def broadcast(message, routing_key):
    """
    docstring
    """
    exchange_name = settings.BROADCAST_EXCHANGE
    creds = pika.PlainCredentials(
        settings.BROADCAST_USERNAME, settings.BROADCAST_PASSWORD)
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=settings.BROADCAST_HOST, credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    channel.basic_publish(exchange=exchange_name,
                          routing_key=routing_key, body=json.dumps(message))
    connection.close()


@shared_task
def process_gift_cards():
    """
    docstring
    """
    LineItem.objects.filter(Name__icontains='gift card').exclude(Status='Archived').update(
        Status='Archived',
        PrintedQuantity=F('Quantity')
    )
    
    LineItem.objects.filter(Name__icontains='shipping protection').exclude(Status='Archived').update(
        Status='Archived',
        PrintedQuantity=F('Quantity')
    )
