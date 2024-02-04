using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class QuickSearchPage
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
    }

    public class QuickSearchBody
    {
        [JsonPropertyName("page")]
        public QuickSearchPage Page { get; set; }

        [JsonPropertyName("searchTerm")]
        public string SearchTerm { get; set; }

        [JsonPropertyName("orderBy")]
        public string OrderBy { get; set; } = "Status";

        [JsonPropertyName("orderByDirection")]
        public string OrderByDirection { get; set; } = "Ascending";

    }

    public class SalesOrder
    {
        [JsonPropertyName("fulfillmentPlanIds")]
        public ICollection<string> FulfillmentPlanIds { get; set; } = new List<string>();
        [JsonPropertyName("salesOrderId")]
        public string SalesOrderId { get; set; }
        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; }

    }

    public class QuickSearchResponseBody
    {
        [JsonPropertyName("currentPageFulfillmentPlanIds")]
        public ICollection<string> CurrentPageFulfillmentPlanIds { get; set; }


        public ICollection<SalesOrder> SalesOrders { get; set; }
    }
}
