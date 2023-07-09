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

        public void PrintItem(MyLineItem myLineItem)
        {
            if (myLineItem.PrintedQuantity >= myLineItem.Quantity)
            {
                throw new Exception("Cannot print when All items were already Printed!");
            }
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
