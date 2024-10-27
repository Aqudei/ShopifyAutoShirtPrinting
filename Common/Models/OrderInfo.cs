using System.Text.Json.Serialization;

namespace Common.Models
{
    public class OrderInfo : EntityBase
    {

        [JsonPropertyName("BinNumber")]
        public int BinNumber { get; set; }

        [JsonPropertyName("OrderId")]
        public long OrderId { get; set; }

        [JsonPropertyName("Active")]
        public bool Active { get; set; } = true;

        [JsonPropertyName("LabelPrinted")]
        public bool LabelPrinted { get; set; }

        [JsonPropertyName("LabelData")]
        public string LabelData { get; set; }

        [JsonPropertyName("TrackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonPropertyName("InsuranceCost")]
        public double InsuranceCost { get; set; }

        [JsonPropertyName("ShipmentCost")]
        public double ShipmentCost { get; set; }

        [JsonPropertyName("ShipmentId")]
        public int ShipmentId { get; set; }

        [JsonPropertyName("OriginalBinNumber")]
        public int OriginalBinNumber { get; set; }
        
        [JsonPropertyName("Shipping")]
        public string Shipping { get; set; }

        private string _shippingFirstName;
        public string ShippingFirstName
        {
            get => _shippingFirstName;
            set => _shippingFirstName = value;
        }

        private string _shippingLastName;
        public string ShippingLastName
        {
            get => _shippingLastName;
            set => _shippingLastName = value;
        }

        private string _shippingAddress1;
        public string ShippingAddress1
        {
            get => _shippingAddress1;
            set => _shippingAddress1 = value;
        }

        private string _shippingAddress2;
        public string ShippingAddress2
        {
            get => _shippingAddress2;
            set => _shippingAddress2 = value;
        }

        private string _shippingPhone;
        public string ShippingPhone
        {
            get => _shippingPhone;
            set => _shippingPhone = value;
        }

        private string _shippingCity;
        public string ShippingCity
        {
            get => _shippingCity;
            set => _shippingCity = value;
        }

        private string _shippingZip;
        public string ShippingZip
        {
            get => _shippingZip;
            set => _shippingZip = value;
        }

        private string _shippingProvince;
        public string ShippingProvince
        {
            get => _shippingProvince;
            set => _shippingProvince = value;
        }

        private string _shippingCountry;
        public string ShippingCountry
        {
            get => _shippingCountry;
            set => _shippingCountry = value;
        }

        private string _shippingCompany;
        public string ShippingCompany
        {
            get => _shippingCompany;
            set => _shippingCompany = value;
        }

        private string _shippingCountryCode;
        public string ShippingCountryCode
        {
            get => _shippingCountryCode;
            set => _shippingCountryCode = value;
        }

        private string _shippingProvinceCode;
        public string ShippingProvinceCode
        {
            get => _shippingProvinceCode;
            set => _shippingProvinceCode = value;
        }

        private string _shippingFullName;
        public string ShippingFullName
        {
            get => _shippingFullName;
            set => _shippingFullName = value;
        }
    }
}
