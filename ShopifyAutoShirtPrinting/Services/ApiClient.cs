using DryIoc;
using ImTools;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.ViewModels;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    public class BinNumberResponse
    {
        public int BinNumber { get; set; }
    }

    public class ItemProcessingResponse
    {
        public MyLineItem LineItem { get; set; }
        public bool AllItemsPrinted { get; set; }
    }


    public class ApiClient
    {
        private readonly RestClient _client;
        private readonly string _baseUrl = $"{Properties.Settings.Default.ApiBaseUrl}";

        public ApiClient()
        {
            var opts = new RestClientOptions
            {
                BaseUrl = new Uri(_baseUrl),
                Authenticator = new HttpBasicAuthenticator("admin", "Espelimbergo"),
            };

            _client = new RestClient(opts);
        }

        public async Task<MyLineItem[]> ListItemsAsync(Dictionary<string, string> queryParams = null)
        {
            var request = new RestRequest("/api/LineItems/");
            if (queryParams != null && queryParams.Count > 0)
            {
                foreach (var item in queryParams)
                {
                    request = request.AddParameter(item.Key, item.Value);
                }
            }

            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem> UpdateLineItemAsync(MyLineItem myLineItem)
        {
            var request = new RestRequest($"/api/LineItems/{myLineItem.Id}/")
                .AddBody(myLineItem);
            var response = await _client.ExecutePutAsync<MyLineItem>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<Log> AddNewLogAsync(Log log)
        {
            var request = new RestRequest($"/api/Logs/")
                .AddBody(log);

            var response = await _client.ExecutePostAsync<Log>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<Log[]> ListLogsAsync(long myLineItemId)
        {
            var request = new RestRequest($"/api/Logs/?LineItem={myLineItemId}");

            var response = await _client.ExecuteGetAsync<Log[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem> GetLineItemByIdAsync(int lineItemDbId)
        {
            var request = new RestRequest($"/api/LineItems/{lineItemDbId}/");
            var response = await _client.ExecuteGetAsync<MyLineItem>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem> GetItemByLineItemIdAsync(long lineItemId)
        {
            var request = new RestRequest($"/api/LineItems/?LineItemId={lineItemId}");
            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data.FirstOrDefault();
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<OrderInfo> GetOrderInfoBy(Dictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                var request = new RestRequest("/api/Orders/");
                foreach (var kvp in parameters)
                {
                    request = request.AddParameter(kvp.Key, kvp.Value);
                }
                var response = await _client.ExecuteGetAsync<OrderInfo[]>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return response.Data.FirstOrDefault();
                }

                throw new Exception(response.Content ?? response.ErrorMessage);

            }

            throw new ArgumentNullException(nameof(parameters));
        }

        public async Task<OrderInfo[]> ListOrdersInfo(Dictionary<string, string> parameters = null)
        {
            var request = new RestRequest("/api/Orders/");

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var kvp in parameters)
                {
                    request = request.AddParameter(kvp.Key, kvp.Value);
                }
            }

            var response = await _client.ExecuteGetAsync<OrderInfo[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<OrderInfo> AddOrderInfo(OrderInfo orderInfo)
        {
            var request = new RestRequest("/api/Orders/")
                .AddBody(orderInfo);

            var response = await _client.ExecutePostAsync<OrderInfo>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<OrderInfo> UpdateOrderInfo(OrderInfo orderInfo)
        {
            var request = new RestRequest($"/api/Orders/{orderInfo.Id}/")
                .AddBody(orderInfo);

            var response = await _client.ExecutePutAsync<OrderInfo>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);

        }

        public async Task<Bin[]> ListBinsAsync()
        {
            var request = new RestRequest("/api/Bins/");

            var response = await _client.ExecuteGetAsync<Bin[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }


        public async Task ResetDatabase()
        {
            var request = new RestRequest("/api/ResetDatabase/");
            var response = await _client.PostAsync(request);

        }

        public async Task EmptyBinAsync(int binNumber)
        {
            var request = new RestRequest($"/api/Bins/{binNumber}/");
            var response = await _client.ExecuteAsync(request, Method.Delete);
        }

        public async Task<ItemProcessingResponse> ProcessItem(long lineItemDbId)
        {
            var request = new RestRequest($"/api/ItemProcessing/{lineItemDbId}/");
            var response = await _client.ExecutePostAsync<ItemProcessingResponse>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem> CreateLineItemAsync(MyLineItem myLineItem)
        {
            var request = new RestRequest($"/api/LineItems/")
                .AddBody(myLineItem);
            var response = await _client.ExecutePostAsync<MyLineItem>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem[]> ListLineItemsAsync(int[] ids)
        {
            var prams = string.Join("&", ids.Select(i => $"Id={i}"));
            var request = new RestRequest($"/api/ListLineItems/?{prams}");
            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<ItemProcessingResponse> UndoPrintAsync(long lineItemDbId)
        {
            var request = new RestRequest($"/api/ItemProcessing/{lineItemDbId}/", Method.Delete);
            var response = await _client.ExecuteAsync<ItemProcessingResponse>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }
    }
}
