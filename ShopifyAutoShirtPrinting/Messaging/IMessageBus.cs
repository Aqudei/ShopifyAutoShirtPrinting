using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public interface IMessageBus : IDisposable
    {
        bool IsConnected { get; }

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task PublishAsync<T>(
            string topic,
            T message,
            CancellationToken cancellationToken = default);

        IDisposable Subscribe<T>(
            string topic,
            Action<T> handler);
    }
}