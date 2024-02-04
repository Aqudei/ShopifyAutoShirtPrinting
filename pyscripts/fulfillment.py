
import shopify
import dotenv
import os
import csv
import pandas as pd
import json

dotenv.load_dotenv()

SHOPIFY_SHOP_URL = os.environ.get("SHOPIFY_SHOP_URL")
SHOPIFY_API_SECRET = os.environ.get("SHOPIFY_API_SECRET")
SHOPIFY_TOKEN = os.environ.get("SHOPIFY_TOKEN")
SHOPIFY_API_KEY = os.environ.get("SHOPIFY_API_KEY")
API_VERSION = "2023-01"

session = shopify.Session(SHOPIFY_SHOP_URL, API_VERSION, SHOPIFY_TOKEN)
shopify.ShopifyResource.activate_session(session)

if __name__ == "__main__":
    orders = []
    orders_response = shopify.Order.find(
        fulfillment_status='unfulfilled', financial_status='paid')

    page = 1
    while True:
        print(f"On page {page}...")
        for o in orders_response:
            orders.append(o.to_dict())

        if not orders_response.has_next_page():
            break

        orders_response = orders_response.next_page()
        page += 1

    with open("./orders.json", 'wt') as outfile:
        outfile.write(json.dumps(orders))
    # ...
    shopify.ShopifyResource.clear_session()
