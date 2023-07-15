from django.core.management.base import BaseCommand, CommandError, CommandParser
from tlkapi.models import OrderInfo, LineItem
from shopifyhook.models import Hook

class Command(BaseCommand):
    """
    docstring
    """
    def add_arguments(self, parser: CommandParser) -> None:
        parser.add_argument("source")

    def handle(self, *args, **options):
        Hook.objects.update(source=options['source'])
        
