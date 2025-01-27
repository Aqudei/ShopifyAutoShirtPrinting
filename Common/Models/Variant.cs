using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Variant : IEquatable<Variant>
    {
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("product")]
        public Product Product { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("shopify_id")]
        public long ShopifyId { get; set; }

        [JsonPropertyName("option1")]
        public string Option1 { get; set; }

        [JsonPropertyName("option2")]
        public string Option2 { get; set; }

        [JsonPropertyName("option3")]
        public string Option3 { get; set; }


        [JsonPropertyName("has_backprint")]
        public bool HasBackPrint { get; set; }
        public override string ToString()
        {
            return $"{Product.Title} - {Title}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Variant);
        }

        public bool Equals(Variant other)
        {
            return other != null &&
                   Id == other.Id &&
                   ShopifyId == other.ShopifyId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ShopifyId);
        }

        public static bool operator ==(Variant left, Variant right)
        {
            return EqualityComparer<Variant>.Default.Equals(left, right);
        }

        public static bool operator !=(Variant left, Variant right)
        {
            return !(left == right);
        }
    }
}
