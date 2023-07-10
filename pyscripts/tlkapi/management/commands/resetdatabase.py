from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders,reset_database_task
from tlkapi.models import Bin

class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        print("Resetting database...")
        reset_database_task()

        print("Generating Bins...")
        for i in range(64):
            Bin.objects.get_or_create(Number=i)