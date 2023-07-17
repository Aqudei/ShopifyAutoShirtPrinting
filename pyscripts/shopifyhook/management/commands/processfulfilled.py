from django.core.management.base import BaseCommand, CommandError, CommandParser
from tlkapi.models import OrderInfo, LineItem
from shopifyhook.models import Hook
from shopifyhook.tasks import process_hooks

class Command(BaseCommand):
    """
    docstring
    """


    def handle(self, *args, **options):
        process_hooks()
