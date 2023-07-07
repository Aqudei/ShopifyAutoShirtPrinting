using ShopifyEasyShirtPrinting.Data;
using System;
using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.Models
{
    public class Log : EntityBase
    {
        [JsonPropertyName("ChangeDate")]
        public DateTime ChangeDate { get; set; }
        [JsonPropertyName("ChangeStatus")]
        public string ChangeStatus { get; set; }

        [JsonPropertyName("LineItem")]
        public int MyLineItemId { get; set; }

        [JsonIgnore]
        public virtual MyLineItem MyLineItem { get; set; }
    }
}
