﻿using Common.Models;
using NLog;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Api
{

    public class PaginatedResult<T>
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }

        [JsonPropertyName("results")]
        public ICollection<T> Results { get; set; }
    }


    public class ConfigResponse
    {
        [JsonPropertyName("logging_email")]
        public string LoggingEmail { get; set; }

        [JsonPropertyName("logging_password")]
        public string LoggingPassword { get; set; }
    }

    public class BinNumberResponse
    {
        public int BinNumber { get; set; }
    }

    public class ItemProcessingResponse
    {
        public MyLineItem LineItem { get; set; }
        public bool AllItemsPrinted { get; set; }
        public int BinNumber { get; set; }
    }

    public class ApiClient
    {
        private readonly RestClient _client;
        private readonly string _baseUrl;
        private static Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ApiClient(SessionVariables globalVariables)
        {
            if (string.IsNullOrWhiteSpace(globalVariables.ServerUrl))
            {
                return;
            }

            _baseUrl = globalVariables.ServerUrl;

            var opts = new RestClientOptions
            {
                BaseUrl = new Uri(_baseUrl),
                Authenticator = new JwtAuthenticator(globalVariables.AccessToken),
            };


            _client = new RestClient(opts);
        }


        public async Task<MyLineItem[]> ListArchivedItemsAsync(int limit = 800, int offset = 0)
        {
            var request = new RestRequest($"/api/ArchivedItems/")
                .AddQueryParameter("limit", limit)
                .AddQueryParameter("offset", offset);

            var response = await _client.ExecuteGetAsync<PaginatedResult<MyLineItem>>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data.Results.ToArray();
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem[]> FindArchivedItemAsync(string searchText)
        {
            var request = new RestRequest($"/api/ArchivedItems/")
                .AddQueryParameter("search", searchText);

            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        private void WriteToFile(string content)
        {
            File.WriteAllText(@".\\lineitems.json", content);
        }

        public async Task<MyLineItem[]> ListItemsAsync(Dictionary<string, string> queryParams = null)
        {
            if (_client == null)
                return new List<MyLineItem>().ToArray();

            var request = new RestRequest("/api/LineItems/");
            if (queryParams != null && queryParams.Count > 0)
            {
                foreach (var item in queryParams)
                {
                    request = request.AddParameter(item.Key, item.Value);
                }
            }

            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
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
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem> UpdateLineItemStatusAsync(int id, string status)
        {
            var request = new RestRequest($"/api/LineItems/{id}/set_status/")
                .AddParameter("status", status);
            var response = await _client.ExecutePostAsync<MyLineItem>(request);
            if (response.StatusCode == HttpStatusCode.OK)
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
            if (response.StatusCode == HttpStatusCode.Created)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<Log[]> ListLogsAsync(long myLineItemId)
        {

            try
            {
                var request = new RestRequest($"/api/Logs/?LineItem={myLineItemId}");

                var response = await _client.ExecuteGetAsync<Log[]>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.Data;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;
        }

        public async Task<MyLineItem> GetLineItemByIdAsync(long lineItemDbId)
        {
            var request = new RestRequest($"/api/LineItems/{lineItemDbId}/");
            var response = await _client.ExecuteGetAsync<MyLineItem>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<MyLineItem> GetLineItemByDatabaseIdAsync(long lineItemDatabaseId)
        {
            var request = new RestRequest($"/api/LineItems/?Id={lineItemDatabaseId}");
            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
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
                if (response.StatusCode == HttpStatusCode.OK)
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
            if (response.StatusCode == HttpStatusCode.OK)
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
            if (response.StatusCode == HttpStatusCode.Created)
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
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);

        }

        public async Task<Bin[]> ListBinsAsync()
        {
            var request = new RestRequest("/api/Bins/");

            var response = await _client.ExecuteGetAsync<Bin[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<Bin[]> FindBinsAsync(Dictionary<string, string> queryParams = null)
        {
            var request = new RestRequest("/api/Bins/");
            if (queryParams != null && queryParams.Count > 0)
            {
                foreach (var item in queryParams)
                {
                    request = request.AddParameter(item.Key, item.Value);
                }
            }
            var response = await _client.ExecuteGetAsync<Bin[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }


        public ConfigResponse GetConfig()
        {
            if (_client == null)
                return null;

            try
            {
                var request = new RestRequest("/api/Config/");

                var response = _client.ExecuteGet<ConfigResponse>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.Data;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task ResetDatabase()
        {
            var request = new RestRequest("/api/ResetDatabase/");
            var response = await _client.PostAsync(request);

        }

        public async Task EmptyBinAsync(int binNumber)
        {
            var request = new RestRequest($"/api/EmptyBin/{binNumber}/");
            var response = await _client.ExecuteAsync(request, Method.Delete);
        }

        public async Task<ItemProcessingResponse> ProcessItemAsync(long lineItemDbId)
        {
            var request = new RestRequest($"/api/ItemProcessing/{lineItemDbId}/");
            var response = await _client.ExecutePostAsync<ItemProcessingResponse>(request);

            if (response.StatusCode == HttpStatusCode.OK)
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

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<MyLineItem[]> ListLineItemsByIdAsync(int[] ids)
        {
            var prams = string.Join("&", ids.Select(i => $"Id={i}"));
            var request = new RestRequest($"/api/ListLineItems/?{prams}");
            var response = await _client.ExecuteGetAsync<MyLineItem[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<Bin> UpdateBinAsync(Bin bin)
        {
            var request = new RestRequest($"/api/Bins/{bin.Id}/")
                .AddBody(bin);

            var response = await _client.ExecutePutAsync<Bin>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<ItemProcessingResponse> UndoPrintAsync(long lineItemDbId)
        {
            var request = new RestRequest($"/api/ItemProcessing/{lineItemDbId}/", Method.Delete);
            var response = await _client.ExecuteAsync<ItemProcessingResponse>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<Variant[]> ListVariantsAsync(string search)
        {

            var request = new RestRequest("/api/tools/Variants/"); ;

            if (!string.IsNullOrWhiteSpace(search))
            {
                request = request.AddParameter("search", search);
            }

            var response = await _client.ExecuteAsync<Variant[]>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<Variant[]> FetchProductVariantsAsync(int productId)
        {

            var request = new RestRequest($"/api/tools/Products/{productId}/list_variants/"); ;

            var response = await _client.ExecuteAsync<Variant[]>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<PrintRequest[]> FetchPrintRequests()
        {
            if (_client == null)
                return null;

            var request = new RestRequest("/api/PrintRequest/");
            var response = await _client.ExecuteGetAsync<PrintRequest[]>(request);

            if (response.IsSuccessStatusCode && response.IsSuccessful)
            {
                return response.Data;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }


        public async Task RequestPrint(int lineItemId)
        {
            var request = new RestRequest($"/api/LineItems/{lineItemId}/request_print/");

            var response = await _client.ExecutePostAsync(request);

        }

        public async Task<string[]> ListProductTypes()
        {
            var request = new RestRequest($"/api/tools/product_types/");

            var response = await _client.ExecuteGetAsync<string[]>(request);
            if (response.IsSuccessStatusCode && response.IsSuccessful)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task RestoreItemsAsync(IEnumerable<MyLineItem> enumerable)
        {
            var body = new IdArray
            {
                Ids = enumerable.Select(x => x.Id).ToArray()
            };

            var request = new RestRequest("/api/ArchivedItems/Restore/")
                .AddBody(body);

            var response = await _client.ExecutePostAsync(request);

        }

        public async Task<PaginatedResult<T>> FetchPage<T>(string url, Dictionary<string, string> reqParams = null)
        {
            var uri = new Uri(url);
            var request = new RestRequest(uri.PathAndQuery);
            if (reqParams != null)
            {
                foreach (var p in reqParams)
                {
                    request = request.AddParameter(p.Key, p.Value);
                }
            }
            var response = await _client.ExecuteAsync<PaginatedResult<T>>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<PaginatedResult<Product>> ListProductsAsync(Dictionary<string, string> reqParams = null)
        {

            var request = new RestRequest("/api/tools/Products/"); ;

            if (reqParams != null)
            {
                foreach (var p in reqParams)
                {
                    request = request.AddParameter(p.Key, p.Value);
                }
            }

            var response = await _client.ExecuteAsync<PaginatedResult<Product>>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        public async Task<Variant> FindVariant(long? variantId)
        {
            var request = new RestRequest($"/api/tools/FindVariant/{variantId.Value}/");
            var response = await _client.ExecuteGetAsync<Variant>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            return null;
        }

        public async Task<Variant[]> SearchVariants(string searchText)
        {
            var request = new RestRequest($"/api/tools/Variants/?search={searchText}");
            var response = await _client.ExecuteGetAsync<Variant[]>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            return new Variant[] { null };
        }
    }
}