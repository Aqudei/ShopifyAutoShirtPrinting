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
        private string _code;
        private string _description;
        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                SetProperty(ref _isSelected, value);
            }
        }


        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("code")]
        public string Code { get => _code; set => SetProperty(ref _code, value); }
        [JsonPropertyName("description")]
        public string Description { get => _description; set => SetProperty(ref _description, value); }
    }
}
