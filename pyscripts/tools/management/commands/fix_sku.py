from django.core.management.base import BaseCommand, CommandParser
from tools.models import (Product,)
import pandas as pd
import logging
import shopify
from django.conf import settings

logger = logging.getLogger(__name__)
print(__name__)


class Command(BaseCommand):
    """
    Import products from Shopify
    """

    def add_arguments(self, parser: CommandParser) -> None:
        parser.add_argument("source")
        parser.add_argument("--reset", action='store_true')

    def __load_df(self, source: str):
        if source.lower().endswith(".csv"):
            df = pd.read_csv(source)
        else:
            df = pd.read_excel(source)

        return df

    def handle(self, *args, **options):
        if options['reset']:
            Product.objects.update(sku_fixed=False)
        session = shopify.Session(
            settings.SHOP_URL, settings.API_VERSION, settings.PRIVATE_APP_PASSWORD)
        shopify.ShopifyResource.activate_session(session)
        df = self.__load_df(options['source'])
        products = Product.objects.filter(sku_fixed=False)

        try:
            for product in products:
                for variant in product.variant_set.all():
                    product_df = df[(df['Handle'] == product.handle) & (df['Option1 Value'] == variant.option1) & (
                        df['Option2 Value'] == variant.option2) & (df['Option3 Value'] == variant.option3)]
                    product_df = product_df.dropna(
                        subset=['FinalSKU', 'New SKU'])

                    if len(product_df) == 1:
                        product_info = product_df.iloc[0]

                        shopify_variant = shopify.Variant.find(
                            variant.shopify_id)
                        if shopify_variant and shopify_variant.sku != product_info['FinalSKU'] and not product_info['FinalSKU'] in [None, '']:
                            shopify_variant.sku = product_info['FinalSKU']
                            shopify_variant.save()
                            print(f"Updated {shopify_variant.sku} to {product_info['FinalSKU']}")

                product.sku_fixed = True
                product.save()

        except Exception as e:
            logger.error(e)
        finally:
            shopify.ShopifyResource.clear_session()
