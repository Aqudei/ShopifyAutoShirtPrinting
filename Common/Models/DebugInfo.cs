using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class FieldDetail
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class DebugInfo
    {
        [JsonPropertyName("errors")]

        public IEnumerable<FieldDetail> Errors { get; set; } = new List<FieldDetail>();

        [JsonPropertyName("warnings")]

        public IEnumerable<FieldDetail> Warnings { get; set; } = new List<FieldDetail>();
    }
}
