using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CustomDesign
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("design_type")]
        public string DesignType { get; set; }
        [JsonPropertyName("text_value")]
        public string TextValue { get; set; }
        [JsonPropertyName("image_value")]
        public string ImageValue { get; set; }
    }
}
