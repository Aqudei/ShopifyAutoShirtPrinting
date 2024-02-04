using Prism.Mvvm;

namespace ShopifyEasyShirtPrinting.Models
{
    public class OrgFile : BindableBase
    {
        private string _destination;
        public string Filename { get; set; }
        public string Sku { get; set; }

        public string Destination
        {
            get => _destination;
            set => SetProperty(ref _destination, value);
        }


    }
}
