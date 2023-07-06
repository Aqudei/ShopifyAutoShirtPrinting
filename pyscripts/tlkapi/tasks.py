from celery import shared_task
import shopify
from django.conf import settings
from .models import OrderInfo, LineItem, Log

@shared_task
def reset_database_task():
    """
    docstring
    """
    Log.objects.all().delete()
    LineItem.objects.all().delete()
    OrderInfo.objects.all().delete()

    
def process_orders(orders_response):
    """
    docstring
    """
    to_add = []
    for order in orders_response:
        created_order, _ = OrderInfo.objects.get_or_create(OrderId=order.id)

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
