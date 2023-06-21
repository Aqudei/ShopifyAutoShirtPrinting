using Sprache;
using System;
using System.Linq;
using System.Text;

namespace ShopifyEasyShirtPrinting.Models
{
    public class MyQr
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public long? OrderId { get; set; }
        public long? LineItemId { get; set; }
        public string OrderNumber { get; set; }

        public override string ToString()
        {
            return $"OrderId:\t{OrderId}\nLineItemId:\t{LineItemId}\n" +
                $"OrderNumber:\t{OrderNumber}";
        }

        public static MyQr Parse(string detectedQr)
        {
            try
            {

                var base64EncodedBytes = Convert.FromBase64String(detectedQr);
                var decodedString = Encoding.UTF8.GetString(base64EncodedBytes);

                var parts = decodedString.Split(Environment.NewLine.ToCharArray());
                parts = parts.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray().ToArray();

                if (parts.Length < 3)
                {
                    throw new Exception("QR Info Lacking!");
                }

                var orderId = parts[0];
                var lineItemId = parts[1];
                var orderNumber = parts[2];

                return new MyQr
                {
                    LineItemId = long.Parse(lineItemId),
                    OrderNumber = orderNumber,
                    OrderId = long.Parse(orderId)
                };
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}
