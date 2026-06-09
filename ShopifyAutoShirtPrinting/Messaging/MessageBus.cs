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
                Password = _sessionVariables.BroadcastPassword,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange_name, ExchangeType.Fanout);
            
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName,
                              exchange: exchange_name,
                              routingKey: string.Empty);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (channel, eventArg) =>
            {
                var body = eventArg.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Debug.WriteLine($" [x] Received {eventArg.RoutingKey}: {message}");

                try
                {
                    ProcessMessage(eventArg.RoutingKey, message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing message: {ex.Message}");
                }

                await Task.CompletedTask;
            };

            _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        private void ProcessMessage(string routingKey, string message)
        {
            switch (routingKey)
            {
                case "items.updated":
                    ItemsUpdated?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                case "items.added":
                    ItemsAdded?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                case "bins.destroyed":
                    BinsDestroyed?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                case "bins.updated":
                    BinsUpdated?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                case "items.archived":
                    ItemsArchived?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                case "database.reset":
                    DatabaseReset?.Invoke(this, EventArgs.Empty);
                    break;
                case "shipments.updated":
                    ShipmentsUpdated?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                case "shipments.voided":
                    ShipmentsVoided?.Invoke(this, JsonConvert.DeserializeObject<int[]>(message));
                    break;
                default:
                    Debug.WriteLine($"Unhandled routing key: {routingKey}");
                    break;
            }
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
