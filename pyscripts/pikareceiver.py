#!/usr/bin/env python
import pika, sys, os


exchange_name = 'thelonelykids'
def main():
    creds = pika.PlainCredentials('warwick','warwickpass1')
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='170.64.158.123', credentials=creds))
    channel = connection.channel()
    
    channel.exchange_declare(exchange=exchange_name, exchange_type='fanout')
    result = channel.queue_declare(queue='', exclusive=True)
    queue_name = result.method.queue
    channel.queue_bind(exchange=exchange_name, queue=queue_name)


    def callback(ch, method, properties, body):
        print(" [x] Received %r" % body)

    channel.basic_consume(queue=queue_name, on_message_callback=callback, auto_ack=True)

    print(' [*] Waiting for messages. To exit press CTRL+C')
    channel.start_consuming()

if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        print('Interrupted')
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)