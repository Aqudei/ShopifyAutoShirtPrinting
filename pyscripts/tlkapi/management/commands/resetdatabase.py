from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders,reset_database_task

class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        reset_database_task()