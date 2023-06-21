using AutoMapper;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.Data
{
    public class LineRepository : PGSQLRepositoryBase<MyLineItem>, ILineRepository
    {
        public LineRepository(LonelyKidsContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
