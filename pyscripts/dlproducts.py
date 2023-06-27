
import json
import shopify
import dotenv
import os
import csv
import pandas as pd

dotenv.load_dotenv()

SHOPIFY_SHOP_URL = os.environ.get("SHOPIFY_SHOP_URL")
SHOPIFY_TOKEN = os.environ.get("SHOPIFY_TOKEN")
SHOPIFY_API_KEY = os.environ.get("SHOPIFY_API_KEY")
SHOPIFY_API_SECRET = os.environ.get("SHOPIFY_API_SECRET")
API_VERSION = "2023-01"


def get_variants():
    products = shopify.Product.find()

    while True:
        for product in products:
            for variant in product.variants:
                yield variant.__dict__

        if products.has_next_page():
            products = products.next_page()
        else:
            break


if __name__ == "__main__":

    session = shopify.Session(SHOPIFY_SHOP_URL, API_VERSION, SHOPIFY_TOKEN)
    shopify.ShopifyResource.activate_session(session)

    variants = list(get_variants())
    with open("./variants.json", 'wt', newline='') as outfile:
        outfile.write(json.dumps(variants))

    # ...
    shopify.ShopifyResource.clear_session()
