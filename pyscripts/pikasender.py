import pika
creds = pika.PlainCredentials('warwick','warwickpass1')
connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='170.64.158.123', credentials=creds))
channel = connection.channel()

channel.queue_declare(queue='hello')

channel.basic_publish(exchange='', routing_key='hello', body='Hello World!')
print(" [x] Sent 'Hello World!'")
connection.close()