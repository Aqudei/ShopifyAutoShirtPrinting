from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders, populate_info
from django.conf import settings
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

        order = find_order(21911)
        import pdb; pdb.set_trace()
        # populate_info(7014)