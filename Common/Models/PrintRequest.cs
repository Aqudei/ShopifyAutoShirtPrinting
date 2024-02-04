using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class PrintRequest
    {
        [JsonPropertyName("Done")]
        public bool Done { get; set; }

        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }


        [JsonPropertyName("LineItem")]
        public MyLineItem LineItem { get; set; }
        
        [JsonPropertyName("Variant")]
        public Variant Variant { get; set; }
    }
}
