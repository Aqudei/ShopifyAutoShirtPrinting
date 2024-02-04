using System.Text.Json.Serialization;

namespace Common.Models
{
    public abstract class EntityBase
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
    }
}
