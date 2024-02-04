namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class Store
    {
        //"storeId": 22766,
        //"storeName": "ShipStation Manual Store",
        //"marketplaceId": 0,
        //"marketplaceName": "ShipStation",
        //"accountName": null,
        //"email": null,
        //"integrationUrl": null,
        //"active": true,
        //"companyName": "",
        //"phone": "",
        //"publicEmail": "testemail@email.com",
        //"website": "",
        //"refreshDate": "2014-12-03T11:46:11.283",
        //"lastRefreshAttempt": "2014-12-03T11:46:53.433",
        //"createDate": "2014-07-25T11:05:55.307",
        //"modifyDate": "2014-11-12T08:45:20.55",
        //"autoRefresh": false

        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int MarketPlaceId { get; set; }
        public string MarketPlaceName { get; set; }
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string IntegrationUrl { get; set; }
        public bool Active { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string PublicEmail { get; set; }
        public string Website { get; set; }
        public string RefreshDate { get; set; }
        public string LastRefreshAttempt { get; set; }
        public string CreateDate { get; set; }
        public string ModifyDate { get; set; }
        public bool AutoRefresh { get; set; }
    }
}
