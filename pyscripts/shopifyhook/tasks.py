import json
from celery import shared_task
from .models import Hook
import logging
from tlkapi.models import Bin, OrderInfo
from tlkapi.tasks import broadcast
from django.conf import settings

logger = logging.getLogger(__name__)


@shared_task
def process_hooks():
    """
    docstring
    """
    removed_bins_number = []
    
    for hook_data in Hook.objects.filter(processed=False):
        try:
            order = OrderInfo.objects.get(OrderNumber=hook_data.body['order_number'])
            if order.Bin:
                bin = order.Bin
                bin.Active = False
                bin.save()
                order.Bin = None
                order.save()

                removed_bins_number.append(bin.Number)

            hook_data.processed=True
            hook_data.save()
        except OrderInfo.DoesNotExist:
            pass
        except Exception as e:
            break
        
    broadcast.delay(json.dumps(removed_bins_number) ,"bins.destroyed")
