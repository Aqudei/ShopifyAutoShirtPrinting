using System;
using System.Collections.Generic;
using System.Text;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public interface IMessageSubscription
    {
        void Invoke(string json);

    }
}
