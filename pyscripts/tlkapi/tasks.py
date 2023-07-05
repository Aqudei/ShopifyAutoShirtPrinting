from celery import shared_task

@shared_task
def fetch_orders():
    """
    docstring
    """
    print("@fetch_orders()")