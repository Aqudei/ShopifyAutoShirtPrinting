
import shopify
import dotenv
import os
import csv
import pandas as pd

dotenv.load_dotenv()

SHOPIFY_SHOP_URL = os.environ.get("SHOPIFY_SHOP_URL")
SHOPIFY_API_SECRET = os.environ.get("SHOPIFY_API_SECRET")
SHOPIFY_TOKEN = os.environ.get("SHOPIFY_TOKEN")
SHOPIFY_API_KEY = os.environ.get("SHOPIFY_API_KEY")
API_VERSION = "2023-01"

session = shopify.Session(SHOPIFY_SHOP_URL, API_VERSION, SHOPIFY_TOKEN)
shopify.ShopifyResource.activate_session(session)

if __name__ == "__main__":
    for p in shopify.Order.find():
        fos = shopify.FulfillmentOrders.find(order_id=p.id)
        for fo in fos: 
            import pdb
            pdb.set_trace()
            print(fo.to_dict())

    # ...
    shopify.ShopifyResource.clear_session()
