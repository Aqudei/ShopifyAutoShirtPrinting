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
        private readonly LogRespository _logRespository;

        //private readonly ILiteCollection<MyLineItem> _lineItemsCollection;

        public MyPrintService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<MyLineItem> PrintItem(MyLineItem myLineItem)
        {
            await Task.Delay(1);

            if (myLineItem.PrintedQuantity < myLineItem.Quantity)
            {
                var orderInfo = await _apiClient.GetOrderInfoBy(new Dictionary<string, string> { { "OrderId", $"{myLineItem.OrderId}" } });
                if (orderInfo == null)
                {
                    await _apiClient.AddOrderInfo(new OrderInfo
                    {
                        OrderId = myLineItem.OrderId.Value
                    });
                }

                // Do actual printing
                myLineItem.PrintedQuantity += 1;
                myLineItem.Status = "Processed";
                myLineItem.BinNumber = await GetBinAsync(myLineItem.OrderId.Value);
                await _apiClient.UpdateLineItemAsync(myLineItem);


                await _apiClient.AddNewLogAsync(new Log
                {
                    ChangeDate = DateTime.Now,
                    ChangeStatus = "Processed",
                    MyLineItemId = myLineItem.Id
                });
            }

            return await _apiClient.GetLineItemByIdAsync(myLineItem.Id);
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

        private async Task<int> GetBinAsync(long orderId)
        {

            var orders = await _apiClient.ListItemsAsync(new Dictionary<string, string> { { "OrderId", $"{orderId}" } });

            var totalItems = orders.Sum(l => l.Quantity);
            var binNumber = 0;

            if (totalItems > 1)
            {
                var orderInfo = await _apiClient.GetOrderInfoBy(new Dictionary<string, string> { { "OrderId", $"{orderId}" } });

                if (orderInfo != null)
                {
                    binNumber = orderInfo.BinNumber != 0 ? orderInfo.BinNumber : await GetNextAvailableBinNumber();
                    orderInfo.BinNumber = binNumber;
                    await _apiClient.UpdateOrderInfo(orderInfo);
                }
                else
                {
                    throw new Exception("OrderInfo not found!");
                }
            }

            return binNumber;
        }

        private async Task<int> GetNextAvailableBinNumber()
        {
            for (var i = 1; ; i++)
            {

                var orderInfos = await _apiClient.ListOrdersInfo(new Dictionary<string, string> { { "BinNumber", $"{i}" }, { "Active", "1" } });

                if (orderInfos.Any())
                {
                    continue;
                }

                return i;
            }
        }
    }
}
