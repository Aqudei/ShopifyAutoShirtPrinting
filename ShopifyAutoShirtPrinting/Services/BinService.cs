using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ShopifyEasyShirtPrinting.Services
{
    public class BinService
    {
        private readonly IOrderRepository _orderInfoRepository;
        private readonly ILineRepository _lineRepository;

        public BinService(IOrderRepository orderRepository, ILineRepository lineRepository)
        {
            _orderInfoRepository = orderRepository;
            _lineRepository = lineRepository;
        }


        public IEnumerable<Bin> ListBins()
        {
            var orders = _orderInfoRepository.Find(o => o.BinNumber != 0 && o.Active).OrderBy(o => o.BinNumber).ToList();
            foreach (var order in orders)
            {
                var lines = _lineRepository.Find(b => b.BinNumber != 0 && b.OrderId == order.OrderId);
                var bin = new Bin
                {
                    BinNumber = order.BinNumber,
                    Items = new List<MyLineItem>(lines)
                };

                yield return bin;
            }
        }

        public void EmptyBin(int binNumber)
        {
            var orders = _orderInfoRepository.Find(o => o.BinNumber == binNumber && o.Active);
            foreach (var order in orders)
            {
                order.Active = false;
                order.BinNumber = 0;
                _orderInfoRepository.Update(order);
            }

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var lines = _lineRepository.Find(l => orderIds.Contains(l.OrderId.Value));

            foreach (var line in lines)
            {
                line.BinNumber = 0;
                _lineRepository.Update(line);
            }
        }

    }
}
