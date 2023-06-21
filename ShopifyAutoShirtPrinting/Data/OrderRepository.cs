using AutoMapper;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.Data
{
    internal class OrderRepository : PGSQLRepositoryBase<OrderInfo>, IOrderRepository
    {
        public OrderRepository(LonelyKidsContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
