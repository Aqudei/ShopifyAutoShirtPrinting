from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders
from django.conf import settings
import shopify
from tlkapi import myshopify
import logging

logger = logging.getLogger(__name__)
print(__name__)


class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        fetch_orders()