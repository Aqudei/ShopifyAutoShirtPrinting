from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem,Bin
from tlkapi.tasks import fetch_orders

class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        for i in range(64):
            Bin.objects.get_or_create(Number=i)