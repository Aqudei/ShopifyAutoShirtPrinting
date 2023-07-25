from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders, populate_info
from django.conf import settings
from django.db.models import F, Value
import shopify
import logging
from tlkapi.myshopify import find_order


logger = logging.getLogger(__name__)
print(__name__)


class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        pass