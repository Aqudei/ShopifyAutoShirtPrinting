from django.core.management.base import BaseCommand, CommandError, CommandParser
from tlkapi.models import OrderInfo, LineItem,Bin
from tlkapi.tasks import fetch_orders
from django.conf import settings

class Command(BaseCommand):
    """
    docstring
    """
    def add_arguments(self, parser: CommandParser) -> None:
        parser.add_argument("--numbins",default=settings.MAX_BINS, type=int)

    def handle(self, *args, **options):
        for i in range(options['numbins']):
            obj, created = Bin.objects.get_or_create(Number=i)
            if created:
                print(f"Bin #{i} was created")