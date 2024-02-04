using System;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class Log : EntityBase
    {
        [JsonPropertyName("ChangeDate")]
        public DateTime ChangeDate { get; set; }

        public DateTime ChangeDateLocal
        {
            get
            {
                return ChangeDate.ToUniversalTime().ToLocalTime();
            }
        }

        [JsonPropertyName("ChangeStatus")]
        public string ChangeStatus { get; set; }
        [JsonPropertyName("LineItem")]
        public int MyLineItemId { get; set; }
        [JsonIgnore]
        public virtual MyLineItem MyLineItem { get; set; }
    }
}
