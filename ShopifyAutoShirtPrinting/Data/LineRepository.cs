using AutoMapper;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.Data
{
    public class LineRepository : PGSQLRepositoryBase<MyLineItem>, ILineRepository
    {
        public LineRepository(string connectionString, IMapper mapper) : base(connectionString, mapper)
        {
        }
    }
}
