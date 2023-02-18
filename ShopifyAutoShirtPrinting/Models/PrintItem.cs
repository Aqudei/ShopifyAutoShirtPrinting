using Prism.Mvvm;

namespace ShopifyEasyShirtPrinting.Models
{
    public class PrintItem : BindableBase
    {
        private string _status;
        public string Path { get; set; }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string LineItemId { get; set; }
        public string VariantName { get; set; }
        public string OrderNumber { get; set; }
        public string Quantity { get; set; }
        public string Sku { get; set; }
        public string VariantId { get; set; }
        public string VariantTitle { get; set; }
    }
}
