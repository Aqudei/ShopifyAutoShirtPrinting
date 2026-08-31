using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public class MessageSubscription<T> : IMessageSubscription
    {
        private readonly Action<T> _handler;

        public MessageSubscription(Action<T> handler)
        {
            _handler = handler;
        }
        public void Invoke(string json)
        {
            var message =
           JsonConvert.DeserializeObject<T>(json);

            if (message != null)
                _handler(message);
        }
    }
}
