using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CreateShipmentRequestBody
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("OrderNumber")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("PostageProductId")]
        public string PostageProductId { get; set; }

        [JsonPropertyName("TotalWeight")]
        public decimal TotalWeight { get; set; }

        private string _shippingFirstName;
        [JsonPropertyName("ShippingFirstName")]
        public string ShippingFirstName
        {
            get => _shippingFirstName;
            set => _shippingFirstName = value;
        }

        private string _shippingLastName;
        [JsonPropertyName("ShippingLastName")]
        public string ShippingLastName
        {
            get => _shippingLastName;
            set => _shippingLastName = value;
        }

        private string _shippingAddress1;
        [JsonPropertyName("ShippingAddress1")]
        public string ShippingAddress1
        {
            get => _shippingAddress1;
            set => _shippingAddress1 = value;
        }

        private string _shippingAddress2;
        [JsonPropertyName("ShippingAddress2")]
        public string ShippingAddress2
        {
            get => _shippingAddress2;
            set => _shippingAddress2 = value;
        }

        private string _shippingPhone;
        [JsonPropertyName("ShippingPhone")]
        public string ShippingPhone
        {
            get => _shippingPhone;
            set => _shippingPhone = value;
        }

        private string _shippingCity;
        [JsonPropertyName("ShippingCity")]
        public string ShippingCity
        {
            get => _shippingCity;
            set => _shippingCity = value;
        }

        private string _shippingZip;
        [JsonPropertyName("ShippingZip")]
        public string ShippingZip
        {
            get => _shippingZip;
            set => _shippingZip = value;
        }

        private string _shippingProvince;
        [JsonPropertyName("ShippingProvince")]
        public string ShippingProvince
        {
            get => _shippingProvince;
            set => _shippingProvince = value;
        }

        private string _shippingCountry;
        [JsonPropertyName("ShippingCountry")]
        public string ShippingCountry
        {
            get => _shippingCountry;
            set => _shippingCountry = value;
        }

        private string _shippingCompany;
        [JsonPropertyName("ShippingCompany")]
        public string ShippingCompany
        {
            get => _shippingCompany;
            set => _shippingCompany = value;
        }

        private string _shippingCountryCode;
        [JsonPropertyName("ShippingCountryCode")]
        public string ShippingCountryCode
        {
            get => _shippingCountryCode;
            set => _shippingCountryCode = value;
        }

        private string _shippingProvinceCode;
        [JsonPropertyName("ShippingProvinceCode")]
        public string ShippingProvinceCode
        {
            get => _shippingProvinceCode;
            set => _shippingProvinceCode = value;
        }

        private string _shippingFullName;
        [JsonPropertyName("ShippingFullName")]
        public string ShippingFullName
        {
            get => _shippingFullName;
            set => _shippingFullName = value;
        }

        [JsonPropertyName("Shipping")]
        public string Shipping { get; set; }
    }

}
