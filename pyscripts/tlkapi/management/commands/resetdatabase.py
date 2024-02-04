import logging
from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders,reset_database_task
from tlkapi.models import Bin
from tlkapi.tasks import fetch_orders

logger = logging.getLogger(__name__)

class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        logger.info("Resetting database...")
        reset_database_task()

        logger.info("Fetching Orders...")
        fetch_orders()