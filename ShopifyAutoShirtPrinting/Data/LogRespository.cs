using AutoMapper;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.Data
{
    public class LogRespository : PGSQLRepositoryBase<Log>
    {
        public LogRespository(string connectionString, IMapper mapper) : base(connectionString, mapper)
        {
        }
    }
}
