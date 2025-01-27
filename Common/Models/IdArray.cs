using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class IdArray
    {
        [JsonPropertyName("ids")]
        public int[] Ids { get; set; }
    }
}
