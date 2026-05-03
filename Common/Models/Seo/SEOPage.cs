using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models.Seo
{
    public class SEOPage
    {
        [JsonPropertyName("store")]
        public int Store { get; set; }

        [JsonPropertyName("shopify_id")]
        public string ShopifyId { get; set; }

        [JsonPropertyName("handle")]
        public string Handle { get; set; }


        [JsonPropertyName("page_title")]
        public string PageTitle { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("description_html")]
        public string DescriptionHtml { get; set; }



        [JsonPropertyName("url")]
        public Uri Url { get; set; }


        [JsonPropertyName("page_type")]
        public string PageType { get; set; }

        [JsonPropertyName("target_keyword")]
        public string TargetKeyword { get; set; }

        [JsonPropertyName("seo_title")]
        public string SEOTitle { get; set; }

        [JsonPropertyName("meta_description")]
        public string MetaDescription { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }


        [JsonPropertyName("image_alt_text")]
        public string ImageAltText { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; }

        [JsonPropertyName("reason_flagged")]
        public List<string> ReasonFlagged { get; set; }
    }
}
