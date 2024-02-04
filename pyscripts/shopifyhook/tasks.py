import json
from celery import shared_task
from .models import Hook
import logging
from tlkapi.models import Bin, OrderInfo, LineItem, Log
from tlkapi.tasks import broadcast
from django.conf import settings

logger = logging.getLogger(__name__)


@shared_task
def archived_cancelled():
    """
    docstring
    """
    removed_bins_number = set()
    archived_items = set()

    cancelled_orders = Hook.objects.filter(
        processed=False, event='orders/cancelled')

    for cancelled_order in cancelled_orders:
        order_number = cancelled_order.body['order_number']
        try:
            order = OrderInfo.objects.get(OrderNumber=order_number)
            order.LineItems.update(
                Status='Archived'
            )

            for line_item in order.LineItems:
                archived_items.add(line_item.id)

            if order.Bin:
                removed_bins_number.add(order.Bin.Number)

                order.Bin.Active = False
                order.Bin.Notes = ''
                order.Bin.save()
                
                order.OriginalBinNumber = order.Bin.Number
                order.Bin = None
                order.save()

            Log.objects.bulk_create([Log(LineItem=line, ChangeStatus="Cancelled/Archived")
                                    for line in LineItem.objects.filter(Order=order)])

            cancelled_order.processed = True
            cancelled_order.save()
        except OrderInfo.DoesNotExist as e:
            cancelled_order.processed = True
            cancelled_order.save()

        except Exception as e:
            logger.exception(e)

    if settings.BROADCAST_ENABLED:
        if removed_bins_number and len(removed_bins_number) > 0:
            broadcast.delay(removed_bins_number, "bins.destroyed")
        if archived_items and len(archived_items) > 0:
            broadcast.delay(archived_items, "items.archived")


@shared_task
def process_hooks(forced=False):
    """
    docstring
    """
    removed_bins_number = set()
    archived_items = set()

    print(f"Processing wehook data using {forced} option...")
    if not forced:
        hook_data_list = Hook.objects.filter(
            processed=False, event='orders/fulfilled')
    else:
        hook_data_list = Hook.objects.filter(event='orders/fulfilled')

    for hook_data in hook_data_list:
        try:
            order_query = OrderInfo.objects.filter(
                OrderNumber=hook_data.body['order_number'])

            if order_query.exists():

                order = order_query.first()

                logger.info(f"Found order {order}")

                bin = order.Bin

                if bin:
                    
                    bin.Active = False
                    bin.Notes = ''
                    bin.save()

                    order.Bin = None
                    order.OriginalBinNumber = bin.Number
                    order.save()

                    removed_bins_number.add(bin.Number)

                LineItem.objects.filter(Order=order).update(
                    Status='Archived'
                )

                Log.objects.bulk_create([Log(LineItem=line, ChangeStatus="Fulfilled/Archived")
                                        for line in LineItem.objects.filter(Order=order)])

                for id in LineItem.objects2.active_items().filter(Order=order).values_list('Id', flat=True):
                    archived_items.add(id)

            hook_data.processed = True
            hook_data.save()
        except Exception as e:
            logger.exception(e)
            break

    if settings.BROADCAST_ENABLED:
        if removed_bins_number and len(removed_bins_number) > 0:
            broadcast.delay(list(removed_bins_number), "bins.destroyed")
        if archived_items and len(archived_items) > 0:
            broadcast.delay(list(archived_items), "items.archived")
