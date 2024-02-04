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
    public class BinService
    {
        private readonly ApiClient _apiClient;
        private readonly IMapper _mapper;

        public BinService(ApiClient apiClient, IMapper mapper)
        {
            _apiClient = apiClient;
            this._mapper = mapper;
        }


        public async Task<BinViewModel[]> ListBinsAsync()
        {
            var bins = await _apiClient.ListBinsAsync();

            return bins.Select(_mapper.Map<BinViewModel>).ToArray();
        }

        public async Task EmptyBinAsync(int binNumber)
        {
            await _apiClient.EmptyBinAsync(binNumber);
        }
    }
}
