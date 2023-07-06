using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ShopifyEasyShirtPrinting.Services
{
    public class MyPrintService
    {
        private readonly ApiClient _apiClient;

        //private readonly ILiteCollection<MyLineItem> _lineItemsCollection;

        public MyPrintService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<MyLineItem> PrintItem(MyLineItem myLineItem)
        {
            if (myLineItem.PrintedQuantity >= myLineItem.Quantity)
            {
                throw new Exception("Cannot print when All items were already Printed!");
            }

            var orderInfo = await _apiClient.GetOrderInfoBy(new Dictionary<string, string> { { "OrderId", $"{myLineItem.OrderId}" } });
            if (orderInfo == null)
            {
                throw new Exception($"Missing OrderInfo / Id: {orderInfo.OrderId}");
            }

            // Do actual printing

            var updatedLineItem = await _apiClient.ProcessItem(myLineItem.Id);

            //myLineItem.PrintedQuantity += 1;
            //myLineItem.Status = "Processed";
            //myLineItem.BinNumber = updatedLineItem.BinNumber;
            //var updatedLineItem = await _apiClient.UpdateLineItemAsync(myLineItem);

            //await _apiClient.AddNewLogAsync(new Log
            //{
            //    ChangeDate = DateTime.Now,
            //    ChangeStatus = "Processed",
            //    MyLineItemId = myLineItem.Id
            //});

            return updatedLineItem;
        }


        public bool AreAllItemsPrinted(long orderId, IEnumerable<MyLineItem> lineItems)
        {
            foreach (var orderLineItem in lineItems)
            {
                if (orderLineItem.PrintedQuantity < orderLineItem.Quantity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
