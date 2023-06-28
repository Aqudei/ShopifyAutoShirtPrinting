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
        private readonly ShipStationApi _shipStationApi;
        private readonly IOrderRepository _orderInfoRepository;
        private readonly ILineRepository _lineRepository;
        //private readonly ILiteCollection<MyLineItem> _lineItemsCollection;

        public MyPrintService(ShipStationApi shipStationApi, IOrderRepository orderRepository, ILineRepository lineRepository)
        {
            _lineRepository = lineRepository;
            _shipStationApi = shipStationApi;
            _orderInfoRepository = orderRepository;
        }

        public async Task<MyLineItem> PrintItem(MyLineItem myLineItem)
        {
            await Task.Delay(1);

            if (myLineItem.PrintedQuantity < myLineItem.Quantity)
            {
                var orderInfo = _orderInfoRepository.Get(o => o.OrderId == myLineItem.OrderId);
                if (orderInfo == null)
                {
                    _orderInfoRepository.Add(new OrderInfo
                    {
                        OrderId = myLineItem.OrderId.Value
                    });
                }

                // Do actual printing
                myLineItem.PrintedQuantity += 1;
                myLineItem.Status = "Processed";
                myLineItem.BinNumber = GetBin(myLineItem.OrderId.Value);
                _lineRepository.Update(myLineItem);
            }

            return _lineRepository.Get(l => l.Id == myLineItem.Id);
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

        private int GetBin(long orderId)
        {

            var orders = _lineRepository.Find(o => o.OrderId == orderId);

            var totalItems = orders.Sum(l => l.Quantity);
            var binNumber = 0;

            if (totalItems > 1)
            {
                var orderInfo = _orderInfoRepository.Get(b => b.OrderId == orderId);

                if (orderInfo != null)
                {
                    binNumber = orderInfo.BinNumber != 0 ? orderInfo.BinNumber : GetNextAvailableBinNumber();
                    orderInfo.BinNumber = binNumber;
                    _orderInfoRepository.Update(orderInfo);
                }
                else
                {
                    throw new Exception("OrderInfo not found!");
                }
            }

            return binNumber;
        }

        private int GetNextAvailableBinNumber()
        {
            for (var i = 1; ; i++)
            {
                var orderInfos = _orderInfoRepository.Find(b => b.BinNumber == i && b.Active);

                if (orderInfos.Any())
                {
                    continue;
                }

                return i;
            }
        }
    }
}
