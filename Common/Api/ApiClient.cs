using Common.Models;
using Common.Models.Harmonisation;
using NLog;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        public async Task<MyLineItem[]> ListItemsAsync(Dictionary<string, string> queryParams = null, Store store = null)
        {
            if (_client == null)
                return new List<MyLineItem>().ToArray();

            var request = new RestRequest("/api/LineItems/");

            if (store != null)
            {
                request = request.AddHeader("Use-Store", store.Id);
            }

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

        public async Task<OrderInfo> GetOrderOrderByAsync(Dictionary<string, string> parameters)
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

        public async Task<MyLineItem> CreateLineItemForStoreAsync(Store store, MyLineItem myLineItem)
        {
            var request = new RestRequest($"/api/LineItems/")
                .AddBody(myLineItem)
                .AddHeader("Use-Store", store.Id);

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



        public async Task<Shipment> CreateShipmentAsync(CreateShipmentRequestBody shipment)
        {
            var request = new RestRequest($"/shipping/CreateShipment/")
                .AddBody(shipment);

            var response = await _client.ExecutePostAsync<Shipment>(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Logger.Error(response.Content ?? response.ErrorMessage);
                return null;
            }

            return response.Data;
        }

        public async Task<IEnumerable<PostageProduct>> ListPostageProductsAsync()
        {
            var request = new RestRequest($"/shipping/postages");

            var response = await _client.ExecuteGetAsync<IEnumerable<PostageProduct>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            return response.Data;
        }

        public async Task ResetBackPrintsAsync()
        {
            var request = new RestRequest($"/api/tools/Variants/reset_backprints/");

            var response = await _client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return;

            throw new Exception(response.ErrorMessage ?? response.Content);

        }
        public async Task SetBackPrintAsync(string sku)
        {
            var request = new RestRequest($"/api/tools/Variants/set_backprint/")
                .AddQueryParameter("sku", sku);

            var response = await _client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return;

            throw new Exception(response.ErrorMessage ?? response.Content);
        }

        #region ShippingEndpoints

        public async Task<IEnumerable<Shipment>> FetchShipmentsByAsync(int offset = 0, int limit = 500, Dictionary<string, string> getParameters = null)
        {
            var request = new RestRequest("/shipping/shipments/")
                .AddQueryParameter("limit", limit);

            if (getParameters != null && getParameters.Count > 0)
            {
                foreach (var param in getParameters)
                {
                    request.AddQueryParameter(param.Key, param.Value.ToString());
                }
            }

            var response = await _client.ExecuteGetAsync<PaginatedResult<Shipment>>(request);
            if (response.StatusCode == HttpStatusCode.OK) { return response.Data.Results; }

            return new Shipment[] { null };

        }

        public async Task<ShipmentOrder> CreateManifestAsync()
        {
            var request = new RestRequest("/shipping/CreateManifest/");

            var response = await _client.ExecutePostAsync<ShipmentOrder>(request);
            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            return response.Data;
        }

        public async Task<IEnumerable<PackagingType>> ListPackagingTypesAsync()
        {
            var request = new RestRequest($"/shipping/packages");

            var response = await _client.ExecuteGetAsync<IEnumerable<PackagingType>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
                return new List<PackagingType>();

            return response.Data;
        }

        public async Task<ShipmentOrder> GetShipmentOrderByIdAsync(int shipmentOrderId)
        {
            var request = new RestRequest($"/shipping/GetShipmentOrder/{shipmentOrderId}/");
            var response = await _client.ExecuteGetAsync<ShipmentOrder>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            return null;
        }

        public async Task<IEnumerable<Shipment>> GetShipmentsByIdsAsync(int[] shipmentDbIds)
        {
            var request = new RestRequest($"/shipping/shipments/")
                .AddQueryParameter("ids", string.Join(",", shipmentDbIds));

            var response = await _client.ExecuteGetAsync<IEnumerable<Shipment>>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task<Shipment> GetShipmentByAsync(Dictionary<string, string> getParameters)
        {
            if (getParameters != null && getParameters.Count > 0)
            {
                var request = new RestRequest("/shipping/shipments/");
                foreach (var kvp in getParameters)
                {
                    request = request.AddQueryParameter(kvp.Key, kvp.Value);
                }
                var response = await _client.ExecuteGetAsync<IEnumerable<Shipment>>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.Data.First();
                }

                throw new Exception(response.Content ?? response.ErrorMessage);

            }

            throw new ArgumentNullException(nameof(getParameters));
        }

        public async Task<Shipment> VoidLabelForShipmentAsync(int shipmentId)
        {
            var request = new RestRequest($"/shipping/shipments/{shipmentId}/void_label/");

            var response = await _client.ExecutePostAsync<Shipment>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(response.Content);
            }

            return response.Data;
        }

        public async Task<IEnumerable<HSN>> ListHarmonizationsAsync()
        {
            var request = new RestRequest($"/shipping/harmonizations/");

            var response = await _client.ExecuteGetAsync<IEnumerable<HSN>>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            throw new Exception(response.Content ?? response.ErrorMessage);
        }

        public async Task SaveHSNAsync(HSN hsn)
        {
            RestResponse<HSN> response;
            if (hsn.Id <= 0)
            {
                var request = new RestRequest($"/shipping/harmonizations/")
                              .AddJsonBody(hsn);
                response = await _client.ExecutePostAsync<HSN>(request);
            }
            else
            {
                var request = new RestRequest($"/shipping/harmonizations/{hsn.Id}/")
                              .AddJsonBody(hsn);
                response = await _client.ExecutePutAsync<HSN>(request);
            }

            if (!response.IsSuccessful) { throw new Exception(response.Content); }
        }

        public async Task SaveBulkHSNs(IEnumerable<HSN> hsns)
        {
            var request = new RestRequest($"/shipping/HSNBulk/")
                .AddBody(hsns);

            var response = await _client.ExecutePostAsync(request);
            if (response.StatusCode != HttpStatusCode.OK) { throw new Exception(response.Content); }

        }

        public async Task DeleteHsnAsync(HSN item)
        {
            var request = new RestRequest($"/shipping/harmonizations/{item.Id}/");
            var response = await _client.ExecuteDeleteAsync(request);

            if (!response.IsSuccessful)
            {
                Logger.Error(response.Content);
                Debug.WriteLine(response.Content);

            }
        }

        public async Task<IEnumerable<string>> ListShippingTypeAsync()
        {
            var request = new RestRequest($"/shipping/types");
            var response = await _client.ExecuteGetAsync<IEnumerable<string>>(request);
            return response.Data;
        }

        #endregion
    }
}
