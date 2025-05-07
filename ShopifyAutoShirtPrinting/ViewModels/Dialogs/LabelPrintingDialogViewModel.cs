using AutoMapper;
using Common.Api;
using Common.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs;

public class LabelPrintingDialogViewModel : PageBase, IDialogAware, INotifyDataErrorInfo
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private DelegateCommand<string> _dialogCommand;
    private int? _binNumber;
    private string _orderNumber;
    private string _customerName;
    private string _customerEmail;
    private string _message;
    private string _notes;
    private readonly ApiClient _apiClient;
    private readonly IMapper _mapper;
    private readonly SessionVariables _globalVariables;
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


    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateProperty(string propertyName, object value)
    {
        // Clear previous errors for this property
        if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }

        // Apply validation rules
        var errors = new List<string>();

        switch (propertyName)
        {
            case nameof(ShippingAddress1):
            case nameof(ShippingAddress2):
                if (!string.IsNullOrWhiteSpace(value?.ToString()) && value?.ToString()?.Length > 40)
                    errors.Add("Address length cannot exceed 40 characters.");
                break;
            default:
                break;
        }

        // Add errors to dictionary and raise event
        if (errors.Count > 0)
        {
            _errors[propertyName] = errors;
            OnErrorsChanged(propertyName);
        }
    }


    private string _shippingAddress1;

    public string ShippingAddress1
    {
        get => _shippingAddress1;
        set
        {
            SetProperty(ref _shippingAddress1, value);
            ValidateProperty(nameof(ShippingAddress1), _shippingAddress1);
        }
    }

    private string _shippingAddress2;

    public string ShippingAddress2
    {
        get => _shippingAddress2;
        set
        {
            SetProperty(ref _shippingAddress2, value);
            ValidateProperty(nameof(ShippingAddress2), _shippingAddress2);
        }
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
    private string _shipping;
    private PackagingType _selectedPackagingType;
    private string _packageType;
    private bool _isBusy;

    public string PostageProductId
    {
        get => _postageProductId;
        set => SetProperty(ref _postageProductId, value);
    }

    public string Shipping
    {
        get => _shipping;
        set => SetProperty(ref _shipping, value);
    }

    public decimal TotalWeight
    {
        get => _totalWeight;
        set => SetProperty(ref _totalWeight, value);
    }

    public ObservableCollection<MyLineItem> LineItems { get; set; } = new();

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public ObservableCollection<PackagingType> PackagingTypes { get; set; } = new();

    public PackagingType SelectedPackagingType
    {
        get => _selectedPackagingType;
        set => SetProperty(ref _selectedPackagingType, value);
    }

    public DelegateCommand<string> DialogCommand => _dialogCommand ??= new DelegateCommand<string>(OnCommand);

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private async Task<bool> HandleAusPostLabelPrinting(CreateShipmentRequestBody createShipmentBody)
    {
        await _dispatcher.BeginInvoke(ShipmentErrors.Clear);

        try
        {
            await _dispatcher.BeginInvoke(() => IsBusy = true);

            var shipmentInfo = await _apiClient.CreateShipmentAsync(createShipmentBody);
            if (shipmentInfo == null) return false;

            var timeStart = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(20);

            while (DateTime.Now - timeStart <= timeout)
            {
                if (shipmentInfo.HasLabel && shipmentInfo.Label != null)
                {
                    var labelUrl = shipmentInfo.Label.IsAbsoluteUri
                        ? shipmentInfo.Label
                        : new Uri(new Uri(_globalVariables.ServerUrl, UriKind.Absolute), shipmentInfo.Label);

                    var labelPath = Path.Combine(_globalVariables.PdfsPath, Path.GetFileName(labelUrl.ToString()));
                    var destination = await PrintHelpers.DownloadRemoteFileToLocalAsync(labelUrl, labelPath);

                    if (!string.IsNullOrWhiteSpace(destination) && File.Exists(destination))
                        PrintHelpers.PrintPdf(destination, Properties.Settings.Default.LabelPrinter);

                    return false;
                }

                if (shipmentInfo.DebugInfo?.Errors?.Any() == true)
                {
                    foreach (var error in shipmentInfo.DebugInfo.Errors)
                        await _dispatcher.BeginInvoke(() => ShipmentErrors.Add(error));
                    return true;
                }

                if (shipmentInfo.DebugInfo?.Warnings?.Any() == true)
                    foreach (var warning in shipmentInfo.DebugInfo.Warnings)
                        await _dispatcher.BeginInvoke(() => ShipmentErrors.Add(warning));

                await Task.Delay(3000);
                shipmentInfo = await _apiClient.GetShipmentByAsync(new Dictionary<string, string>
                    { { "OrderNumber", createShipmentBody.OrderNumber } });
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return true;
        }
        finally
        {
            await _dispatcher.BeginInvoke(() => IsBusy = false);
        }
    }


    private async void OnCommand(string cmd)
    {
        if (cmd.ToUpper() == "YES")
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
        }
        else if (cmd.ToUpper() == "AUSPOST")
        {
            var shipment = _mapper.Map<CreateShipmentRequestBody>(this);
            shipment.PostageProductId = SelectedPostage.PostageProductId;


            ValidateProperty(nameof(ShippingAddress1), _shippingAddress1);
            ValidateProperty(nameof(ShippingAddress2), _shippingAddress2);

            if (HasErrors)
                return;

            var retry = await Task.Run(() => HandleAusPostLabelPrinting(shipment));
            if (!retry)
            {
                var dlgParams = new DialogParameters { { "auspost", true } };
                RequestClose?.Invoke(new DialogResult(ButtonResult.Yes, dlgParams));
            }
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

    public LabelPrintingDialogViewModel(ApiClient apiClient, IMapper mapper, SessionVariables globalVariables)
    {
        _globalVariables = globalVariables;
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.TryGetValue<string>("Message", out var message)) Message = message;

        if (parameters.TryGetValue<string>("OrderNumber", out var orderNumber))
        {
            LineItems.Clear();
            OrderNumber = orderNumber;
            Task.Run(LoadData);
        }
    }

    public string PackageType
    {
        get => _packageType;
        set => SetProperty(ref _packageType, value);
    }

    public PostageProduct SelectedPostage
    {
        get => _selectedPostageProduct;
        set => SetProperty(ref _selectedPostageProduct, value);
    }

    public ObservableCollection<PostageProduct> Postages { get; set; } = new();

    private async Task LoadData()
    {
        var orderNumberParams = new Dictionary<string, string>() { { "OrderNumber", $"{OrderNumber}" } };
        var lineItems = await _apiClient.ListItemsAsync(orderNumberParams);
        var shipment = await _apiClient.GetShipmentByAsync(orderNumberParams);
        var postages = await _apiClient.ListPostageProductsAsync();
        var packaging = await _apiClient.ListPackagingTypesAsync();

        if (lineItems != null && lineItems.Any())
        {
            var lineItems0 = lineItems.FirstOrDefault();
            var shippingLine = lineItems0.Shipping;

            var customerName = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.Customer)).FirstOrDefault()?.Customer;
            var customerEmail = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.CustomerEmail)).FirstOrDefault()
                ?.CustomerEmail;

            await _dispatcher.InvokeAsync(() =>
            {
                _mapper.Map(shipment, this);

                LineItems.AddRange(lineItems);
                BinNumber = lineItems.FirstOrDefault()?.BinNumber;
                Notes = lineItems.FirstOrDefault()?.Notes;
                CustomerName = customerName;
                CustomerEmail = customerEmail;

                Postages.AddRange(postages);
                PackagingTypes.AddRange(packaging);

                TotalWeight = shipment.TotalWeight > 0 ? shipment.TotalWeight : lineItems.Sum(l => l.Grams);
                
                SelectedPostage = Postages.FirstOrDefault(p =>
                    p.PostageShippings.Select(pp => pp.Shipping?.ToLower()).Contains(shippingLine.ToLower()));
                SelectedPackagingType = PackagingTypes.FirstOrDefault(pk => pk.Code == PackageType);
            });
        }
    }

    public IEnumerable GetErrors(string propertyName)
    {
        return _errors.TryGetValue(propertyName, out var errors) ? errors : null;
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
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public ObservableCollection<FieldDetail> ShipmentErrors { get; set; } = new();

    public event Action<IDialogResult> RequestClose;
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
}