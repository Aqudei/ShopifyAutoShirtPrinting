from datetime import datetime
import psycopg2
import shopify
from decouple import config
shop_url, api_version, private_app_password = config(
    'SHOPIFY_SHOP_URL'),  config('SHOPIFY_API_VERSION'), config('SHOPIFY_TOKEN')


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
        self.conn = psycopg2.connect(database="thelonelykids", user="postgres",
                                     password="Espelimbergo_122289", host="localhost", port="5432")
        self.cursor = self.conn.cursor()
        self.cursor.execute("SET search_path TO public")

    def fetch_existing_lines(self, order_id):
        """
        docstring
        """
        q = 'SELECT * FROM "MyLineItems" WHERE "OrderId"=%s'
        self.cursor.execute(q, (order_id,))
        columns = list([c[0] for c in self.cursor.description])
        items = self.cursor.fetchall()

        for item in items:
            item_dict = {}
            for c, i in zip(columns, item):
                item_dict[c] = i
            yield item_dict['LineItemId'], item_dict

    def process_orders(self, orders_response):
        """
        docstring
        """
        to_add = []
        for order in orders_response:
            existing_lines = dict(
                {k: v for k, v in self.fetch_existing_lines(order.id)})

            for line in order.line_items:
                if not line.id in existing_lines:
                    to_add.append((order, line))

        for order_item, line_item in to_add:
            shipping_line = order_item.shipping_lines[0].code

            values = (f"{order_item.order_number}", 0, line_item.sku, line_item.name, line_item.variant_id,
                      line_item.variant_title, line_item.id, line_item.quantity, 0, line_item.fulfillment_status or '',
                      order_item.financial_status, f"{order_item.customer.first_name} {order_item.customer.last_name}",
                      order_item.customer.email, order_item.note or '', order.id,  "Pending", shipping_line)
            q = """INSERT INTO public."MyLineItems" ("OrderNumber", "BinNumber", "Sku", "Name", "VariantId", "VariantTitle", "LineItemId", "Quantity", "PrintedQuantity","FulfillmentStatus", "FinancialStatus", "Customer", "CustomerEmail", "Notes", "OrderId", "Status", "Shipping") VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"""
            print("Parameter values: ", values)
            self.cursor.execute(q, values)
            print("Row count: ", self.cursor.rowcount)

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
        
        self.conn.commit()

    def cleanup(self):
        """
        docstring
        """
        self.conn.close()
        shopify.ShopifyResource.clear_session()


if __name__ == "__main__":
    updater = Updater()
    updater.fetch_orders()
    updater.cleanup()
