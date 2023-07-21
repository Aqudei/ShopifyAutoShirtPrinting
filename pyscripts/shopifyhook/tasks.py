import json
from celery import shared_task
from .models import Hook
import logging
from tlkapi.models import Bin, OrderInfo, LineItem
from tlkapi.tasks import broadcast
from django.conf import settings

logger = logging.getLogger(__name__)

@shared_task
def process_hooks(forced=False):
    """
    docstring
    """
    removed_bins_number = []
    archived_items = []

    print(f"Processing wehook data using {forced} option...")
    if not forced:
        hook_data_list = Hook.objects.filter(processed=False, event='orders/fulfilled')
    else:
        hook_data_list = Hook.objects.filter(event='orders/fulfilled')

    for hook_data in hook_data_list:
        try:
            order = OrderInfo.objects.filter(
                OrderNumber=hook_data.body['order_number']).first()
            
            if order:
                
                logger.info(f"Found order {order}")

                if order.Bin:
                    bin = order.Bin
                    bin.Active = False
                    bin.save()
                    order.Bin = None
                    order.save()
                    removed_bins_number.append(bin.Number)

                LineItem.objects.filter(Order=order).update(
                    Status='Archived'
                )

                for id in LineItem.objects.filter(Order=order).values_list('Id', flat=True):
                    archived_items.append(id)

            hook_data.processed = True
            hook_data.save()
        except Exception as e:
            logger.error(e)
            break

    if settings.BROADCAST_ENABLED:
        if removed_bins_number and len(removed_bins_number)>0:
            broadcast.delay(removed_bins_number, "bins.destroyed")
        if archived_items and len(archived_items)>0:
            broadcast.delay(archived_items, "items.archived")
