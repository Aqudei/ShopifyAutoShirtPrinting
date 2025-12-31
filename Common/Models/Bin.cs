using System.Text.Json.Serialization;

namespace Common.Models
{
    public class Bin
    {
        private string _notes;

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("Number")]
        public int BinNumber { get; set; }
        public string OrderNumber { get; set; }
        public MyLineItem[] LineItems { get; set; }

        [JsonPropertyName("Notes")]
        public string Notes { get; set; }

        [JsonPropertyName("StoreName")]
        public string StoreName { get; set; }
    }
}
