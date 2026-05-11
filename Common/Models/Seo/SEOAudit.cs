#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Common.Models.Seo
{
    public class ScoreBreakdown
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }


        [JsonPropertyName("issues")]
        public string[] Issues{ get; set; }


        [JsonPropertyName("details")]
        public Dictionary<string,object>? Details { get; set; }
    }

    public class SEOAudit
    {
        [JsonPropertyName("page")]
        public int PageId { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("breakdown")]
        public ScoreBreakdown[]? Breakdown { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }
    }
}
