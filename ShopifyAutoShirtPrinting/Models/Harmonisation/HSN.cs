using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Models.Harmonisation
{
    public class HSN : BindableBase
    {
        private string code;
        private string description;
        private bool selected;

        public bool Selected { get => selected; set => SetProperty(ref selected, value); }
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("code")]
        public string Code { get => code; set => SetProperty(ref code, value); }
        [JsonPropertyName("description")]
        public string Description { get => description; set => SetProperty(ref description, value); }
    }
}
