from django.db.models.signals import post_save
from django.dispatch import receiver
from .models import LineItem
from tlkapi import tasks

@receiver(post_save, sender=LineItem)
def lineitem_updated(sender, instance, created, **kwargs):
    pass
    # if not created:
    #     tasks.broadcast_change.delay([instance.Id])