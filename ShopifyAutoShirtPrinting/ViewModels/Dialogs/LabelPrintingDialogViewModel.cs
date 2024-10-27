using AutoMapper;
using Common.Api;
using Common.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    internal class LabelPrintingDialogViewModel : PageBase, IDialogAware
    {
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        private DelegateCommand<string> _dialogCommand;
        private int? _binNumber;
        private string _orderNumber;
        private string _customerName;
        private string _customerEmail;
        private string _message;
        private string _notes;
        private readonly ApiClient _apiClient;
        private readonly IMapper _mapper;
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
        private PostageProduct _selectedPostageProduct;

        public string ShippingFullName
        {
            get => _shippingFullName;
            set => SetProperty(ref _shippingFullName, value);
        }

        private decimal _totalWeight;
        private string _postageProductId;

        public string PostageProductId
        {
            get { return _postageProductId; }
            set { SetProperty(ref _postageProductId, value); }
        }

        public decimal TotalWeight
        {
            get { return _totalWeight; }
            set { SetProperty(ref _totalWeight, value); }
        }

        public ObservableCollection<MyLineItem> LineItems { get; set; } = new();
        public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

        public DelegateCommand<string> DialogCommand => _dialogCommand ??= new DelegateCommand<string>(OnCommand);

        private void OnCommand(string cmd)
        {
            if (cmd.ToUpper() == "YES")
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
            }
            else if (cmd.ToUpper() == "AUSPOST")
            {
                var shipment = _mapper.Map<Shipment>(this);
                shipment.PostageProductId = SelectedPostage.PostageProductId;

                var dlgParams = new DialogParameters
                {
                    { "auspost", true },
                    { "shipment", shipment}
                };


                RequestClose?.Invoke(new DialogResult(ButtonResult.Yes, dlgParams));
            }
            else
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.No));
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public LabelPrintingDialogViewModel(ApiClient apiClient, IMapper mapper)
        {
            _apiClient = apiClient;
            _mapper = mapper;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Message", out var message))
            {
                Message = message;
            }

            if (parameters.TryGetValue<string>("OrderNumber", out var orderNumber))
            {
                LineItems.Clear();
                OrderNumber = orderNumber;
                Task.Run(LoadData);
            }
        }

        public PostageProduct SelectedPostage { get => _selectedPostageProduct; set => SetProperty(ref _selectedPostageProduct, value); }
        public ObservableCollection<PostageProduct> Postages { get; set; } = new();

        private async Task LoadData()
        {
            var prams = new Dictionary<string, string>() { { "OrderNumber", $"{OrderNumber}" } };
            var lineItems = await _apiClient.ListItemsAsync(prams);
            var orderInfo = await _apiClient.GetOrderInfoBy(new Dictionary<string, string> { { "OrderNumber", OrderNumber } });
            var postages = await _apiClient.ListPostageProductsAsync();


            if (lineItems != null && lineItems.Any())
            {
                var lineItems0 = lineItems.FirstOrDefault();
                var shippingLine = lineItems0.Shipping;

                var customerName = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.Customer)).FirstOrDefault()?.Customer;
                var customerEmail = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.CustomerEmail)).FirstOrDefault()?.CustomerEmail;

                await _dispatcher.InvokeAsync(() =>
                {

                    _mapper.Map(orderInfo, this);

                    LineItems.AddRange(lineItems);
                    BinNumber = lineItems.FirstOrDefault()?.BinNumber;
                    Notes = lineItems.FirstOrDefault()?.Notes;
                    CustomerName = customerName;
                    CustomerEmail = customerEmail;


                    Postages.AddRange(postages);

                    TotalWeight = lineItems.Sum(l => l.Grams);
                    SelectedPostage = Postages.FirstOrDefault(p => p.ShippingOption?.ToLower() == shippingLine.ToLower());
                });
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set => SetProperty(ref _customerEmail, value);
        }

        public int? BinNumber
        {
            get => _binNumber;
            set => SetProperty(ref _binNumber, value);
        }

        public string OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }



        public override string Title => "Print Shipment Label";

        public event Action<IDialogResult> RequestClose;
    }
}
