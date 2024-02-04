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
        session = shopify.Session(
            settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
        shopify.ShopifyResource.activate_session(session)

        q = Q(ProductImage__isnull=True) | Q(ProductImage='')
        lineitems = LineItem.objects.filter(q)
        
        for lineitem in lineitems:
            try:
                variant = Variant.objects.get(shopify_id=lineitem.VariantId)
                time.sleep(2)
                sh_variant = shopify.Variant.find(variant.shopify_id)
                
                if not sh_variant.image_id or sh_variant.image_id==0:
                    time.sleep(2)
                    sh_product = shopify.Product.find(variant.product.shopify_id)
                    if len(list(sh_product.images)) <=0:
                        continue
                    image = list(sh_product.images)
                    variant.image_id =  image[0].id
                else:    
                    variant.image_id =  sh_variant.image_id
                    time.sleep(2)
                    sh_product = shopify.Product.find(variant.product.shopify_id)
                    image = list([img for img in sh_product.images if img.id==sh_variant.image_id])
                
                variant.save()
                lineitem.ProductImage = image[0].src
                lineitem.save()
                
            except Variant.DoesNotExist as e:
                print(f"{e} - LineItem := {lineitem}")
            except:
                pass
                
        shopify.ShopifyResource.clear_session()