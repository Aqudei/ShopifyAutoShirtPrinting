import json
from django.core.management.base import BaseCommand, CommandError, CommandParser
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders
from django.conf import settings
import shopify
from tlkapi import myshopify
import logging
import csv
import pandas as pd

logger = logging.getLogger(__name__)

print(__name__)


class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        try:
            session = shopify.Session(
                    settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
            shopify.ShopifyResource.activate_session(session)

            with open("./products-config.csv",'wt') as outfile:
                writer = csv.writer(outfile)
                writer.writerow(())
                products_json = [p.to_dict() for p in products]
                outfile.write(json.dumps(products_json))
                products = shopify.Product.find()
        except Exception as e:
            logger.error(e)
        finally:
            shopify.ShopifyResource.clear_session()

