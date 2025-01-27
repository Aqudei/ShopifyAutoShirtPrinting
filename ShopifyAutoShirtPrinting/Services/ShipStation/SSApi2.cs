using OpenQA.Selenium;
using RestSharp;
using ShopifyEasyShirtPrinting.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class SSApi2
    {
        private readonly RestClient _client;
        private ReadOnlyCollection<Cookie> _seleniumCookies;

        public SSApi2()
        {
            _client = new RestClient("https://ship12.shipstation.com");
        }

        public async Task<QuickSearchResponseBody> QuickSearchAsync(string orderNumber)
        {
            var searchTerm = orderNumber.StartsWith("#") ? orderNumber : $"#{orderNumber}";
            var resource_url = "/api/ordergrid/shipmentmode/quicksearch/";
            var request = new RestRequest(resource_url)
                .AddBody(new QuickSearchBody
                {
                    SearchTerm = searchTerm,
                    Page = new QuickSearchPage
                    {
                        PageNumber = 1,
                        PageSize = 500
                    }
                })
                .AttachCookies(_seleniumCookies);
            var response = await _client.ExecutePostAsync<QuickSearchResponseBody>(request);
            return response.Data;
        }

        public void InitCookies(IWebDriver driver)
        {
            _seleniumCookies = driver.Manage().Cookies.AllCookies;
        }

    }
}
