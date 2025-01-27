using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.Data
{
    public abstract class EntityBase
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
    }
}
