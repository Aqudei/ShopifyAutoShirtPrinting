using AutoMapper;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.Data
{
    internal class OrderRepository : PGSQLRepositoryBase<OrderInfo>, IOrderRepository
    {
        public OrderRepository(string connectionString, IMapper mapper) : base(connectionString, mapper)
        {
        }
    }
}
