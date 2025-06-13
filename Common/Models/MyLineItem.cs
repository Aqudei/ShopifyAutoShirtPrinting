using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class MyLineItem : EntityBase
    {
        [JsonPropertyName("Store")]
        public long? Store { get; set; }

        [JsonPropertyName("OrderNumber")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("Sku")]
        public string Sku { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("VariantId")]
        public long? VariantId { get; set; }
        [JsonPropertyName("VariantTitle")]
        public string VariantTitle { get; set; }
        [JsonPropertyName("LineItemId")]
        public long? LineItemId { get; set; }
        [JsonPropertyName("Quantity")]
        public int? Quantity { get; set; }
        [JsonPropertyName("FulfillmentStatus")]
        public string FulfillmentStatus { get; set; }
        [JsonPropertyName("FinancialStatus")]
        public string FinancialStatus { get; set; }
        [JsonPropertyName("Customer")]
        public string Customer { get; set; }
        [JsonPropertyName("CustomerEmail")]
        public string CustomerEmail { get; set; }

        [JsonPropertyName("DateModified")]
        public DateTime? DateModified { get; set; }

        [JsonPropertyName("ProductImage")]
        public string ProductImage { get; set; }

        [JsonPropertyName("Notes")]
        public string Notes { get; set; }


        [JsonPropertyName("OrderId")]
        public long? OrderId { get; set; }

        [JsonPropertyName("PrintedQuantity")]
        public int PrintedQuantity { get; set; }

        [JsonPropertyName("BinNumber")]
        public int? BinNumber { get; set; }

        [JsonPropertyName("OriginalBinNumber")]
        public int OriginalBinNumber { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("Shipping")]
        public string Shipping { get; set; }

        [JsonPropertyName("ForPickup")]
        public bool ForPickup { get; set; }

        [JsonPropertyName("Grams")]
        public decimal Grams { get; set; }

        [JsonPropertyName("Length")]
        public decimal Length { get; set; }
        [JsonPropertyName("Width")]

        public decimal Width { get; set; }
        [JsonPropertyName("Height")]
        public decimal Height { get; set; }


        //[JsonPropertyName("DesignText")]
        //public string DesignText { get; set; }
        //public override string ToString()
        //{
        //    return $"#{OrderNumber} - {Sku} - {VariantTitle} - {Name}";
        //}

        [JsonPropertyName("Properties")]
        public Dictionary<string, object> Properties
        {
            get; set;
        }

        [JsonPropertyName("Designs")]
        public IEnumerable<CustomDesign> Designs { get; set; }


    }
}