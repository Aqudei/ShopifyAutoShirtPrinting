from celery import shared_task
import shopify
from django.conf import settings

@shared_task
def fetch_orders():
    """
    docstring
    """
    session = shopify.Session(
        settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
    shopify.ShopifyResource.activate_session(session)
    
    