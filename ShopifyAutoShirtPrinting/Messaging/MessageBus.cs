using Common.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public static class MessageTopics
    {
        public const string ShipmentsUpdated = "shipments.updated";
        public const string ShipmentsVoided = "shipments.voided";
        public const string ItemsUpdated = "items.updated";
        public const string ItemsAdded = "items.added";
        public const string ItemsArchived = "items.archived";
        public const string BinsUpdated = "bins.updated";
        public const string BinsDestroyed = "bins.destroyed";
        public const string DatabaseReset = "database.reset";
    }
    public sealed class MessageBus : IMessageBus
    {
        private readonly SessionVariables _sessionVariables;

        private IConnection? _connection;
        private IChannel? _channel;
        private string? _queueName;

        private bool _disposed;

        private readonly ConcurrentDictionary<
            string,
            ConcurrentDictionary<Guid, IMessageSubscription>>
            _subscriptions = new();

        public bool IsConnected =>
            _connection is { IsOpen: true } &&
            _channel is { IsOpen: true };

        public MessageBus(SessionVariables sessionVariables)
        {
            _sessionVariables = sessionVariables;
        }

        public async Task ConnectAsync(
            CancellationToken cancellationToken = default)
        {
            if (IsConnected)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _sessionVariables.BroadcastHost,
                UserName = _sessionVariables.BroadcastUsername,
                Password = _sessionVariables.BroadcastPassword
            };

            _connection = await factory.CreateConnectionAsync(
                cancellationToken);

            _channel = await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            var exchangeName = _sessionVariables.BroadcastExchange;

            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: false,
                autoDelete:false,
                cancellationToken: cancellationToken);

            // Every desktop client gets its own temporary queue.
            var queue = await _channel.QueueDeclareAsync(
                queue: string.Empty,
                durable: false,
                exclusive: true,
                autoDelete: true,
                cancellationToken: cancellationToken);

            _queueName = queue.QueueName;

            await _channel.QueueBindAsync(
                queue: _queueName,
                exchange: exchangeName,
                routingKey: string.Empty,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += OnMessageReceivedAsync;

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: cancellationToken);

            Debug.WriteLine(
                $"RabbitMQ connected. Queue: {_queueName}");
        }

        private Task OnMessageReceivedAsync(object sender,BasicDeliverEventArgs args)
        {
            var topic = args.RoutingKey;

            var json = Encoding.UTF8.GetString(
                args.Body.ToArray());

            if (_subscriptions.TryGetValue(
                    topic,
                    out var subscriptions))
            {
                foreach (var subscription in subscriptions.Values)
                {
                    try
                    {
                        subscription.Invoke(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Error processing '{topic}': {ex}");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task PublishAsync<T>(
            string topic,
            T message,
            CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
                throw new InvalidOperationException(
                    "Message bus is not connected.");

            var exchangeName =
                _sessionVariables.BroadcastExchange;

            var json = JsonConvert.SerializeObject(message);

            var body = Encoding.UTF8.GetBytes(json);

            await _channel!.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: topic,
                body: body,
                cancellationToken: cancellationToken);
        }

        public IDisposable Subscribe<T>(string topic, Action<T> handler)
        {
            var subscriptions = _subscriptions.GetOrAdd(
                topic,
                _ => new ConcurrentDictionary<Guid, IMessageSubscription>());

            var id = Guid.NewGuid();

            subscriptions[id] =
                new MessageSubscription<T>(handler);

            return new Subscription(() =>
            {
                if (_subscriptions.TryGetValue(
                        topic,
                        out var topicSubscriptions))
                {
                    topicSubscriptions.TryRemove(id, out _);

                    if (topicSubscriptions.IsEmpty)
                    {
                        _subscriptions.TryRemove(
                            topic,
                            out _);
                    }
                }
            });
        }

        private static Type GetMessageType(Delegate handler)
        {
            return handler.Method
                .GetParameters()[0]
                .ParameterType;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private int _disposed;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(
                        ref _disposed,
                        1) == 0)
                {
                    _unsubscribe();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Error disposing MessageBus: {ex}");
            }

            _channel = null;
            _connection = null;
        }
    }
}
