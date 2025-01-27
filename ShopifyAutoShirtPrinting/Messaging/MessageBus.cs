using Common.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public class MessageBus : IMessageBus
    {
        private IConnection _connection;
        private IModel _channel;
        private bool disposedValue;
        private readonly SessionVariables _sessionVariables;

        public event EventHandler<int[]> BinsDestroyed;
        public event EventHandler<int[]> BinsUpdated;
        public event EventHandler<int[]> ItemsUpdated;
        public event EventHandler<int[]> ItemsAdded;
        public event EventHandler<int[]> ItemsArchived;
        public event EventHandler<int[]> ShipmentsUpdated;
        public event EventHandler<int[]> ShipmentsVoided;

        public event EventHandler DatabaseReset;

        public MessageBus(SessionVariables sessionVariables)
        {
            _sessionVariables = sessionVariables;

            var exchange_name = _sessionVariables.BroadcastExchange;

            var factory = new ConnectionFactory
            {
                //HostName = Properties.Settings.Default.ServerHost,                
                HostName = _sessionVariables.BroadcastHost,
                UserName = _sessionVariables.BroadcastUsername,
                Password = _sessionVariables.BroadcastPassword
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange_name, ExchangeType.Fanout);
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName,
                              exchange: exchange_name,
                              routingKey: string.Empty);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, eventArg) =>
            {
                var body = eventArg.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Debug.WriteLine($" [x] Received {message}");

                switch (eventArg.RoutingKey)
                {
                    case "items.updated":
                        {
                            var lineItemsIds = JsonConvert.DeserializeObject<int[]>(message);
                            ItemsUpdated?.Invoke(this, lineItemsIds);
                            break;
                        }
                    case "items.added":
                        {
                            var lineItemsIds = JsonConvert.DeserializeObject<int[]>(message);
                            ItemsAdded?.Invoke(this, lineItemsIds);
                            break;
                        }
                    case "bins.destroyed":
                        {
                            var binNumbers = JsonConvert.DeserializeObject<int[]>(message);
                            BinsDestroyed?.Invoke(this, binNumbers);
                            break;
                        }
                    case "bins.updated":
                        {
                            var binNumbers = JsonConvert.DeserializeObject<int[]>(message);
                            BinsUpdated?.Invoke(this, binNumbers);
                            break;
                        }
                    case "items.archived":
                        {
                            var lineItemsIds = JsonConvert.DeserializeObject<int[]>(message);
                            ItemsArchived?.Invoke(this, lineItemsIds);
                            break;
                        }
                    case "database.reset":
                        {
                            DatabaseReset?.Invoke(this, null);
                            break;
                        }
                    case "shipments.updated":
                        {
                            var shipmentIds = JsonConvert.DeserializeObject<int[]>(message);
                            ShipmentsUpdated?.Invoke(this, shipmentIds);
                            break;
                        }
                    case "shipments.voided":
                        {
                            var shipmentIds = JsonConvert.DeserializeObject<int[]>(message);
                            ShipmentsVoided?.Invoke(this, shipmentIds);
                            break;
                        }
                    default:
                        break;
                }


            };

            _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _channel.Close();
                    _channel.Dispose();
                    _connection.Close();
                    _connection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageBus()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
