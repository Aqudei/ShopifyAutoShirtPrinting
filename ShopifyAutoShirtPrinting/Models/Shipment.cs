using Common.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Models
{
    internal class Shipment : BindableBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private bool _manifested;
        public bool Manifested
        {
            get => _manifested;
            set => SetProperty(ref _manifested, value);
        }

        private bool _hasLabel;
        public bool HasLabel
        {
            get => _hasLabel;
            set => SetProperty(ref _hasLabel, value);
        }

        private string _shipmentId;
        public string ShipmentId
        {
            get => _shipmentId;
            set => SetProperty(ref _shipmentId, value);
        }

        private string _orderNumber;
        public string OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        private string _shippingAddress1;
        public string ShippingAddress1
        {
            get => _shippingAddress1;
            set => SetProperty(ref _shippingAddress1, value);
        }

        private string _shippingAddress2;
        public string ShippingAddress2
        {
            get => _shippingAddress2;
            set => SetProperty(ref _shippingAddress2, value);
        }

        private string _shipmentReference;
        public string ShipmentReference
        {
            get => _shipmentReference;
            set => SetProperty(ref _shipmentReference, value);
        }

        private string _senderReferences;
        public string SenderReferences
        {
            get => _senderReferences;
            set => SetProperty(ref _senderReferences, value);
        }

        private DateTime? _shipmentCreationDate;
        public DateTime? ShipmentCreationDate
        {
            get => _shipmentCreationDate;
            set => SetProperty(ref _shipmentCreationDate, value);
        }

        private DateTime? _shipmentModifiedDate;
        public DateTime? ShipmentModifiedDate
        {
            get => _shipmentModifiedDate;
            set => SetProperty(ref _shipmentModifiedDate, value);
        }

        private Uri _label;
        public Uri Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        private IEnumerable<ShipmentItem> _shipmentItems;
        public IEnumerable<ShipmentItem> ShipmentItems
        {
            get => _shipmentItems;
            set => SetProperty(ref _shipmentItems, value);
        }

        public string ManifestFileName => $"shipment-manifest-{ShipmentOrder?.OrderRef}.pdf";

        private ShipmentOrder _shipmentOrder;
        public ShipmentOrder ShipmentOrder
        {
            get => _shipmentOrder;
            set => SetProperty(ref _shipmentOrder, value);
        }

        private string _postageProductId;
        public string PostageProductId
        {
            get => _postageProductId;
            set => SetProperty(ref _postageProductId, value);
        }

        private decimal _totalWeight;
        public decimal TotalWeight
        {
            get => _totalWeight;
            set => SetProperty(ref _totalWeight, value);
        }

        private string _shippingFirstName;
        public string ShippingFirstName
        {
            get => _shippingFirstName;
            set => SetProperty(ref _shippingFirstName, value);
        }

        private string _shippingLastName;
        public string ShippingLastName
        {
            get => _shippingLastName;
            set => SetProperty(ref _shippingLastName, value);
        }

        private string _shippingPhone;
        public string ShippingPhone
        {
            get => _shippingPhone;
            set => SetProperty(ref _shippingPhone, value);
        }

        private string _shippingCity;
        public string ShippingCity
        {
            get => _shippingCity;
            set => SetProperty(ref _shippingCity, value);
        }

        private string _shippingZip;
        public string ShippingZip
        {
            get => _shippingZip;
            set => SetProperty(ref _shippingZip, value);
        }

        private string _shippingProvince;
        public string ShippingProvince
        {
            get => _shippingProvince;
            set => SetProperty(ref _shippingProvince, value);
        }

        private string _shippingCountry;
        public string ShippingCountry
        {
            get => _shippingCountry;
            set => SetProperty(ref _shippingCountry, value);
        }

        private string _shippingCompany;
        public string ShippingCompany
        {
            get => _shippingCompany;
            set => SetProperty(ref _shippingCompany, value);
        }

        private string _shippingCountryCode;
        public string ShippingCountryCode
        {
            get => _shippingCountryCode;
            set => SetProperty(ref _shippingCountryCode, value);
        }

        private string _shippingProvinceCode;
        public string ShippingProvinceCode
        {
            get => _shippingProvinceCode;
            set => SetProperty(ref _shippingProvinceCode, value);
        }

        private string _shippingFullName;
        public string ShippingFullName
        {
            get => _shippingFullName;
            set => SetProperty(ref _shippingFullName, value);
        }

        private string _shipping;
        public string Shipping
        {
            get => _shipping;
            set => SetProperty(ref _shipping, value);
        }

        private string _packageType;
        public string PackageType
        {
            get => _packageType;
            set => SetProperty(ref _packageType, value);
        }

        private DateTime? _createdAt;
        public DateTime? CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime? _modifiedAt;
        public DateTime? ModifiedAt
        {
            get => _modifiedAt;
            set => SetProperty(ref _modifiedAt, value);
        }

        private bool _archived;
        public bool Archived
        {
            get => _archived;
            set => SetProperty(ref _archived, value);
        }

        private DebugInfo _debugInfo;
        public DebugInfo DebugInfo
        {
            get => _debugInfo;
            set => SetProperty(ref _debugInfo, value);
        }
    }

}
