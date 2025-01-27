using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Messaging
{
    internal class DummyMessageBus : IMessageBus
    {
        public event EventHandler<int[]> BinsDestroyed;
        public event EventHandler<int[]> BinsUpdated;
        public event EventHandler DatabaseReset;
        public event EventHandler<int[]> ItemsAdded;
        public event EventHandler<int[]> ItemsArchived;
        public event EventHandler<int[]> ItemsUpdated;

        public void Dispose()
        {
        }
    }
}
