import json
from celery import shared_task
from .models import Hook
import logging
from tlkapi.models import Bin, OrderInfo, LineItem
from tlkapi.tasks import broadcast
from django.conf import settings

logger = logging.getLogger(__name__)


@shared_task
def process_hooks():
    """
    docstring
    """
    removed_bins_number = []
    archived_items = []

    print("Processing wehook data...")
    for hook_data in Hook.objects.filter(processed=False, event='orders/fulfilled'):
        try:
            order = OrderInfo.objects.get(
                OrderNumber=hook_data.body['order_number'])
            
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

            archived_items.extend(LineItem.objects.filter(
                Order=order).values_list('Id', flat=True))

            hook_data.processed = True
            hook_data.save()
        except OrderInfo.DoesNotExist:
            hook_data.processed = True
            hook_data.save()
        except Exception as e:
            logger.error(e)
            break

    if settings.BROADCAST_ENABLED:
        broadcast.delay(json.dumps(removed_bins_number), "bins.destroyed")
        broadcast.delay(json.dumps(archived_items), "items.archived")
