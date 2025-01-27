using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ShipmentOrder
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("order_ref")]
        public string OrderRef { get; set; }
        
        [JsonPropertyName("summary")]
        public string OrderSummary { get; set; }

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("order_creation_date")]
        public DateTime? CreationDate { get; set; }

        [JsonPropertyName("total_cost")]
        public decimal TotalCost{ get; set; }

        public string ManifestFileName => $"shipment-manifest-{OrderId}-{OrderRef}.pdf";

    }
}
