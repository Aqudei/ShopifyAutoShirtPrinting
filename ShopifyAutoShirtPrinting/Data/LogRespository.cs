using AutoMapper;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.Data
{
    public class LogRespository : PGSQLRepositoryBase<Log>
    {
        public LogRespository(LonelyKidsContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
