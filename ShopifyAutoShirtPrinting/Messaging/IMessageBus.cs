using System;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public interface IMessageBus : IDisposable
    {
        event EventHandler<int[]> BinsDestroyed;
        event EventHandler<int[]> BinsUpdated;
        event EventHandler DatabaseReset;
        event EventHandler<int[]> ItemsAdded;
        event EventHandler<int[]> ItemsArchived;
        event EventHandler<int[]> ItemsUpdated;
    }
}