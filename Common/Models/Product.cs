﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Product
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("handle")]
        public string Handle { get; set; }
        [JsonPropertyName("shopify_id")]
        public long ShopifyId { get; set; }
        [JsonPropertyName("product_type")]
        public string ProductType { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }


        [JsonPropertyName("has_backprint")]
        public bool HasBackPrint { get; set; }
        public override string ToString()
        {
            return $"{Handle}";
        }

    }
}