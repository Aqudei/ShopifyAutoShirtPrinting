from datetime import datetime
import psycopg2
import shopify
from decouple import config
import logging
import pytz

# Configure the logging
logging.basicConfig(
    level=logging.INFO,  # Set the desired logging level
    format='%(asctime)s [%(levelname)s] %(message)s',  # Define the log message format
    filename='app.log',  # Specify the log file name
    filemode='w'  # Set the mode for opening the log file (default is 'a' for append)
)
# Create a logger
logger = logging.getLogger()

mytimezone = pytz.timezone("Asia/Manila")

shop_url, api_version, private_app_password = config(
    'SHOPIFY_SHOP_URL'),  config('SHOPIFY_API_VERSION'), config('SHOPIFY_TOKEN')

DB_HOST = config('DB_HOST')
DB_PORT = config('DB_PORT')
DB_NAME = config('DB_NAME')
DB_USER = config('DB_USER')
DB_PASS = config('DB_PASS')

class Updater:
    """
    docstring
    """

    def __init__(self):
        """
        docstring
        """
        self.session = shopify.Session(
            shop_url, api_version, private_app_password)
        shopify.ShopifyResource.activate_session(self.session)
        self.conn = psycopg2.connect(database=DB_NAME, user=DB_USER,
                                     password=DB_PASS, host=DB_HOST, port=DB_PORT)
        self.main_cursor = self.conn.cursor()
        self.main_cursor.execute("SET search_path TO public")

    def fetch_existing_lines(self, order_number):
        """
        docstring
        """
        localcursor = self.conn.cursor()
        localcursor.execute("SET search_path TO public")
        q = '''SELECT * FROM "MyLineItems" WHERE "OrderNumber"=%s'''
        localcursor.execute(q, (f"{order_number}",))
        columns = list([c[0] for c in localcursor.description])
        items = localcursor.fetchall()

        for item in items:
            item_dict = {}
            for c, i in zip(columns, item):
                item_dict[c] = i
            yield item_dict['LineItemId'], item_dict

        localcursor.close()

    def process_orders(self, orders_response):
        """
        docstring
        """
        to_add = []
        for order in orders_response:
            existing_lines = dict(
                {k: v for k, v in self.fetch_existing_lines(order.order_number)})
            for line in order.line_items:
                if not line.id in existing_lines:
                    to_add.append((order, line))
        values = []
        for order_item, line_item in to_add:
            shipping_line = order_item.shipping_lines[0].code

            values.append((f"{order_item.order_number}", 0, line_item.sku, line_item.name, line_item.variant_id,
                           line_item.variant_title, line_item.id, line_item.quantity, 0, line_item.fulfillment_status or '',
                           order_item.financial_status, f"{order_item.customer.first_name} {order_item.customer.last_name}",
                           order_item.customer.email, order_item.note or '', order_item.id,  "Pending", shipping_line))

        q = """INSERT INTO public."MyLineItems" ("OrderNumber", "BinNumber", "Sku", "Name", "VariantId", "VariantTitle", "LineItemId", "Quantity", "PrintedQuantity","FulfillmentStatus", "FinancialStatus", "Customer", "CustomerEmail", "Notes", "OrderId", "Status", "Shipping") VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"""
        self.main_cursor.executemany(q, values)
        self.conn.commit()

    def fetch_orders(self):
        """
        docstring
        """
        orders_response = shopify.Order.find(
            fulfillment_status='unfulfilled', financial_status='paid')
        self.process_orders(orders_response)

        while (orders_response.has_next_page()):
            orders_response = orders_response.next_page()
            self.process_orders(orders_response)

    def cleanup(self):
        """
        docstring
        """
        self.conn.close()
        shopify.ShopifyResource.clear_session()


if __name__ == "__main__":
    try:
        updater = Updater()
        updater.fetch_orders()
        updater.cleanup()
    except Exception as e:
        logger.exception(e)

    last_run_date = mytimezone.fromutc(datetime.utcnow())
    logger.info(f"Last Run: {last_run_date}")