using AutoMapper;
using Common.Api;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    public class BinService(ApiClient apiClient, IMapper mapper)
    {
        public async Task<BinViewModel[]> ListBinsAsync()
        {
            var bins = await apiClient.ListBinsAsync();
            return bins.Select(mapper.Map<BinViewModel>).ToArray();
        }

        public async Task EmptyBinAsync(int binNumber)
        {
            await apiClient.EmptyBinAsync(binNumber);
        }
    }
}