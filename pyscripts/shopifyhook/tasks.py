from celery import shared_task
from .models import Hook

@shared_task
def process_hooks():
    """
    docstring
    """
    for data in Hook.objects.filter(processed=False):
        pass