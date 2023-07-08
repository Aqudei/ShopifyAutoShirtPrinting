using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    public class BinService
    {
        private readonly ApiClient _apiClient;

        public BinService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }


        public async Task<Bin[]> ListBinsAsync()
        {
            return await _apiClient.ListBinsAsync();
        }

        public async Task EmptyBinAsync(int? binNumber)
        {
            if (binNumber.HasValue)
            {
                await _apiClient.EmptyBinAsync(binNumber.Value);
            }
        }

    }
}
