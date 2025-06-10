using Common.Models;
using Newtonsoft.Json;
using Prism.Mvvm;
using ShopifyEasyShirtPrinting.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class BinViewModel : BindableBase
    {
        private string _notes;

        public long Id { get; set; }

        public int BinNumber { get; set; }
        public string OrderNumber { get; set; }
        public string StoreName { get; set; }
        public LineItemViewModel[] LineItems { get; set; }

        public string Notes
        {
            get => _notes; set
            {
                SetProperty(ref _notes, value);
                RaisePropertyChanged(nameof(HasNotes));
            }
        }
        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    }
}
