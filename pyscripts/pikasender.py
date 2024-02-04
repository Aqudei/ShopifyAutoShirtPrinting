import json
import pika

def broadcast_change(ids:list[int]):
    """
    docstring
    """
    exchange_name = 'thelonelykids'
    creds = pika.PlainCredentials('warwick','warwickpass1')
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='170.64.158.123', credentials=creds))
    channel = connection.channel()

    channel.exchange_declare(exchange=exchange_name,exchange_type='fanout')
    message = json.dumps(ids)
    channel.basic_publish(exchange=exchange_name, routing_key='items.updated', body=message)
    print(" [x] Sent 'Hello World!'")
    connection.close()