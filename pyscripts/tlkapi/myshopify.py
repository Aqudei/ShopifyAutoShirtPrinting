import shopify
from django.conf import settings
import logging

logger = logging.getLogger(__name__)

def find_order(order_number):
    # fetch_orders()
    session = shopify.Session(
        settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
    shopify.ShopifyResource.activate_session(session)
    try:
        orders = shopify.Order.find(status='any', name=order_number)
        if orders:
            return orders[0]
    except Exception as e:
        logger.error(e)
    finally:
        shopify.ShopifyResource.clear_session()