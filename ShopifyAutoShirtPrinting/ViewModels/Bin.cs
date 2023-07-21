using Newtonsoft.Json;
using Prism.Mvvm;
using ShopifyEasyShirtPrinting.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class Bin : BindableBase
    {
        private string notes;

        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonPropertyName("Number")]
        [JsonProperty("Number")]
        public int BinNumber { get; set; }
        public string OrderNumber { get; set; }
        public MyLineItem[] LineItems { get; set; }

        [JsonPropertyName("Notes")]
        [JsonProperty("Notes")]
        public string Notes
        {
            get => notes; set
            {
                SetProperty(ref notes, value);
                RaisePropertyChanged(nameof(HasNotes));
            }
        }
        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    }
}
