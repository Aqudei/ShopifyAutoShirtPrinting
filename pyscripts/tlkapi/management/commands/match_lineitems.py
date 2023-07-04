from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem


class Command(BaseCommand):
    """
    docstring
    """

    def handle(self, *args, **options):
        total = LineItem.objects.filter(Order__isnull=True).count()

        for idx,line in enumerate(LineItem.objects.filter(Order__isnull=True)):
            print(f"Processing {idx+1} / {total} . . .")
            order = OrderInfo.objects.filter(OrderId=line.OrderId).first()
            if order:
                line.Order = order
                line.save()