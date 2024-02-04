import logging
from django.core.management.base import BaseCommand, CommandError, CommandParser
from tlkapi.models import OrderInfo, LineItem, PrintRequest
from tlkapi.tasks import fetch_orders,reset_database_task
from tlkapi.models import Bin
from tlkapi.tasks import fetch_orders

logger = logging.getLogger(__name__)

class Command(BaseCommand):
    """
    docstring
    """
    
    def add_arguments(self, parser: CommandParser) -> None:
        parser.add_argument("--soft", action='store_true',default=True)

    def handle(self, *args, **options):
        soft = options['soft']
        if soft:
            PrintRequest.objects.filter(Done=False).update(Done=True)
        else:
            PrintRequest.objects.all().delete()
        
        print("Process done.")