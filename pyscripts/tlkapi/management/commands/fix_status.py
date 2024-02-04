import json
from django.core.management.base import BaseCommand, CommandError, CommandParser
from tlkapi.models import OrderInfo, LineItem, Log
from tlkapi.tasks import fetch_orders
from django.conf import settings
import shopify
from tlkapi import myshopify
import logging
import csv
from django.utils import timezone
from datetime import timedelta
import csv

logger = logging.getLogger(__name__)

print(__name__)


class Command(BaseCommand):
    """
    docstring
    """

    def add_arguments(self, parser: CommandParser) -> None:
        parser.add_argument("status")
        parser.add_argument("--check", action='store_true')

    def handle(self, *args, **options):
        self.__undo_one(options['status'], options['check'])

    def __undo_one(self, status, check):
        """
        docstring
        """
        line_items = LineItem.objects.filter(
            Status = status
        )

        for lineitem in line_items:
            logs = Log.objects.filter(LineItem=lineitem).order_by('-ChangeDate')
            latest_log  = logs.first()
            print(f"Logs count: {logs.count()}")

            if logs.count()==1:
                lineitem.Status = 'Pending'
                if not check:
                    lineitem.save()
                print(f"{lineitem} changed to 'Pending'")
            
            if logs.count() > 1:
                last_status = logs[1].ChangeStatus.replace("Updated status to","").strip("' ")
                lineitem.Status = last_status
                print(f"{lineitem} changed to '{last_status}'")

                if not check:
                    lineitem.save()

            if not check:
                latest_log.delete()

    def __tool1(self):
        line_items = LineItem.objects.filter(
            Status = "Need To Order From Supplier",
            DateModified__gte = timezone.now() - timedelta(hours=20)
        )
        for lineitem in line_items:
            logs = Log.objects.filter(LineItem=lineitem).order_by('-ChangeDate')
            latest_log  = logs.first()

            if logs.count()==1:
                lineitem.Status = 'Pending'
                lineitem.save()
                print(f"{lineitem} changed to 'Pending'")
            if logs.count() > 1:
                last_status = logs[1].ChangeStatus.replace("Updated status to","").strip("' ")
                lineitem.Status = last_status
                print(f"{lineitem} changed to '{last_status}'")
                lineitem.save()
            
            latest_log.delete()