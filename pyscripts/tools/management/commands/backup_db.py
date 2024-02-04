from django.core.management.base import BaseCommand
from tools.tasks import backup_db

import logging

logger = logging.getLogger(__name__)
print(__name__)

class Command(BaseCommand):
    """
    Import products from Shopify
    """

    def handle(self, *args, **options):
        backup_db()
