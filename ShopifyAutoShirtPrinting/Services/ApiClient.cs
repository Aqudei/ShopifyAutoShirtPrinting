﻿using DryIoc;
using ImTools;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    public class AvailableBinResponse
    {
        public int AvailableBinNumber { get; set; }
    }

    public class ApiClient
    {
        private readonly RestClient _client;
        private readonly string _baseUrl = "http://170.64.158.123";

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

            return null;
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

            return null;
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

            return null;
        }

        public async Task<IEnumerable<Log>> ListLogsAsync(int myLineItemId)
        {
            var request = new RestRequest($"/api/Logs/?MyLineItemId={myLineItemId}");

            var response = await _client.ExecuteGetAsync<IEnumerable<Log>>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        public async Task<MyLineItem> GetLineItemByIdAsync(int lineItemDbId)
        {
            var request = new RestRequest($"/api/LineItems/{lineItemDbId}/");
            var response = await _client.ExecuteGetAsync<MyLineItem>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            return null;
        }

        public async Task<MyLineItem> GetItemByLineItemIdAsync(long lineItemId)
        {
            var request = new RestRequest($"/api/LineItems/?LineItemId={lineItemId}");
            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data.FirstOrDefault();
            }

            return null;
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
            }

            return null;
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

            return null;
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

            return null;
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

            return null;
        }

        public async Task<Bin[]> ListBinsAsync()
        {
            var request = new RestRequest("/api/Bins/");

            var response = await _client.ExecuteGetAsync<Bin[]>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            return null;
        }


        public async Task ReetDatabase()
        {
            var request = new RestRequest("/api/ResetDatabase/");
            var response = await _client.PostAsync(request);
        }

        public async Task EmptyBinAsync(int binNumber)
        {
            var request = new RestRequest($"/api/Bins/{binNumber}/");
            var response = await _client.ExecutePostAsync(request);
        }

        public async Task<AvailableBinResponse> GetNextAvailableBin()
        {
            var request = new RestRequest("/api/Bins/Available/");
            var response = await _client.ExecuteGetAsync<AvailableBinResponse>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }

            return null;
        }
    }
}