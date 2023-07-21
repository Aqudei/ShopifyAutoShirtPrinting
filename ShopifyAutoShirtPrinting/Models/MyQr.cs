using Sprache;
using System;
using System.Linq;
using System.Text;

namespace ShopifyEasyShirtPrinting.Models
{
    public class MyQr
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public long LineItemDatabaseId { get; set; }
        public string OrderNumber { get; set; }

        public override string ToString()
        {
            return $"LineItemId:\t{LineItemDatabaseId}\n" +
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

                if (parts.Length < 2)
                {
                    throw new Exception("QR Info Lacking!");
                }

                var lineItemDatabaseId = parts[0];
                var orderNumber = parts[1];

                return new MyQr
                {
                    LineItemDatabaseId = int.Parse(lineItemDatabaseId),
                    OrderNumber = orderNumber,
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
