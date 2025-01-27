using Common.Api;
using Common.Models;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace ShopifyEasyShirtPrinting.Services
{
    public class MyPrintService(ApiClient apiClient)
    {
        //private readonly ILiteCollection<MyLineItem> _lineItemsCollection;

        public async Task PrintItem(LineItemViewModel myLineItem)
        {
            if (myLineItem.PrintedQuantity >= myLineItem.Quantity)
            {
                throw new Exception("Cannot print when All items were already Printed!");
            }


            await apiClient.RequestPrint(myLineItem.Id);
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
