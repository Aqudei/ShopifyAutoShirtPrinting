
import shopify
import dotenv
import os
import csv
import pandas as pd
import requests
import json

dotenv.load_dotenv()


SHIPSTATION_API_KEY = os.environ.get("SHIPSTATION_API_KEY")
SHIPSTATION_API_SECRET = os.environ.get("SHIPSTATION_API_SECRET")
BASE_URL = 'https://ssapi.shipstation.com'


def list_carriers():
    """
    docstring
    """
    response = requests.get(
        f"{BASE_URL}/carriers", auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET))
    return response.json()


def list_packages():
    """
    docstring
    """
    params = {
        "carrierCode": "australia_post"
    }
    response = requests.get(
        f"{BASE_URL}/carriers/listpackages", params=params, auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET))
    return response.json()


def list_services():
    """
    docstring
    """
    params = {
        "carrierCode": "australia_post"
    }
    response = requests.get(
        f"{BASE_URL}/carriers/listservices", params=params, auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET))
    return response.json()


def find_order(orderNumber):
    """
    docstring
    """
    params = {
        "orderNumber": orderNumber
    }
    response = requests.get(
        f"{BASE_URL}/orders", params=params, auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET))
    return response.json()


def list_orders():
    """
    docstring
    """
    params = {
        "orderStatus": "awaiting_shipment"
    }
    response = requests.get(
        f"{BASE_URL}/orders", params=params, auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET))
    return response.json()


def write_json(data, output):
    """
    docstring
    """
    with open(output, 'wt') as outfile:
        outfile.write(json.dumps(data, indent=1))


def list_stores():
    """
    docstring
    """
    params = {
        "orderStatus": "awaiting_shipment"
    }
    response = requests.get(
        f"{BASE_URL}/stores", auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET))

    return response.json()


def search(orderNumber):
    """
    docstring
    """
    body = {
        "page": {
            "pageNumber": 1,
            "pageSize": 250
        },
        "searchTerm": f"{orderNumber}"
    }
    url = 'https://ship12.shipstation.com/api/ordergrid/shipmentmode/quicksearch'
    response = requests.post(
        f"{url}", auth=(SHIPSTATION_API_KEY, SHIPSTATION_API_SECRET), json=body)
    print(response.text)
    return response

if __name__ == "__main__":
    response = search("19225")
