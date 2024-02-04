from io import StringIO
import time
from django.core.management.base import BaseCommand, CommandError
from tlkapi.models import OrderInfo, LineItem
from tlkapi.tasks import fetch_orders
from django.db.models import Q

from django.conf import settings
import shopify
from tlkapi import myshopify
import logging

from tools.models import Product, Variant

logger = logging.getLogger(__name__)
print(__name__)


class Command(BaseCommand):

        
    def handle(self, *args, **options):
        """
        docstring
        """
        q = Q(ProductImage__isnull=True) | Q(ProductImage='')
        lineitems = LineItem.objects.filter(q)
        
        for lineitem in lineitems:
            try:
                variant = Variant.objects.get(shopify_id=lineitem.VariantId)
                if lineitem.ProductImage in (None,''):     
                    lineitem.ProductImage = variant.image_src
                    lineitem.save()
                    
            except Variant.DoesNotExist as e:
                print(f"{e} - {lineitem}")