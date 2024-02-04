using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class ShipStationApi
    {
        private readonly RestClient _client;
        private const string ShipStationApiKey = "846d2815c9264cc3869db3530e5b2c78";
        private const string ShipStationApiSecret = "6e55fc0b7b3140d095a3339578a64c6e";

        public ShipStationApi()
        {
            var config = new ConfigureRestClient(options =>
            {
                options.Authenticator = new HttpBasicAuthenticator(ShipStationApiKey, ShipStationApiSecret);
            });
            _client = new RestClient("https://ssapi.shipstation.com", config, configureSerialization: s => s.UseNewtonsoftJson());
        }

        public async Task<List<Carrier>> ListCarriers()
        {
            var req = new RestRequest("/carriers");
            var response = await _client.ExecuteAsync<List<Carrier>>(req, Method.Get);
            return response.Data;
        }

        public async Task<IEnumerable<Package>> ListPackages(string carrierCode)
        {
            var req = new RestRequest("/carriers/listpackages")
                .AddParameter("carrierCode", carrierCode);

            var response = await _client.ExecuteAsync<IEnumerable<Package>>(req, Method.Get);
            return response.Data;
        }

        public async Task<IEnumerable<Service>> ListServices(string carrierCode)
        {
            var req = new RestRequest("/carriers/listservices")
                .AddParameter("carrierCode", carrierCode);

            var response = await _client.ExecuteAsync<List<Service>>(req, Method.Get);
            return response.Data;
        }

        public async Task<CreateLabelResponse> CreateLabel(CreateLabelRequest createLabelRequestData)
        {
            var req = new RestRequest("/orders/createlabelfororder")
                .AddBody(createLabelRequestData);

            var response = await _client.ExecutePostAsync<CreateLabelResponse>(req);
            return response.Data;
        }

        public async Task<Order> GetOrderByNumber(string orderNumber)
        {
            var req = new RestRequest("/orders")
                .AddParameter("orderNumber", orderNumber);
            var response = await _client.ExecuteAsync<ListOrdersResponse>(req, Method.Get);
            return response.StatusCode == HttpStatusCode.OK ? response.Data.Orders.FirstOrDefault() : null;
        }

        public async Task<Store[]> ListStoresAsync()
        {
            var req = new RestRequest("/stores");
            var response = await _client.ExecuteAsync<Store[]>(req, Method.Get);
            return response.Data;
        }

        public async void RefreshStores()
        {
            var stores = await ListStoresAsync();
            foreach (var store in stores)
            {
                var req = new RestRequest("/stores/refreshstore")
                    .AddParameter("storeId", store.StoreId);
                var response = await _client.ExecuteGetAsync(req);
            }
        }

        public async Task<ListOrdersResponse> ListShippedOrdersAsync(Dictionary<string, string> parameters)
        {
            var request = new RestRequest("/orders")
                .AddParameter("orderStatus", "shipped");

            foreach (var keyValuePair in parameters)
            {
                request.AddParameter(keyValuePair.Key, keyValuePair.Value);
            }

            var response = await _client.ExecuteGetAsync<ListOrdersResponse>(request);
            return response.StatusCode != HttpStatusCode.OK ? null : response.Data;
        }

    }
}
