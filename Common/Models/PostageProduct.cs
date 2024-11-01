using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class PostageShipping
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        public string Shipping { get; set; }
    }

    public class PostageProduct
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("postage_type")]
        public string PostageType { get; set; }

        [JsonPropertyName("postage_product_id")]
        public string PostageProductId { get; set; }

        [JsonPropertyName("PostageShippings")]
        public IEnumerable<PostageShipping> PostageShippings { get; set; }

    }
}
