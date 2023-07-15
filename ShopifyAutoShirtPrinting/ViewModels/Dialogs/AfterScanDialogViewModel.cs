using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class AfterScanDialogViewModel : PageBase, IDialogAware
    {
        private string _title;
        private string _orderNumber;
        private int? _binNumber;
        private string _customerName;
        private string _customerEmail;
        public ObservableCollection<MyLineItem> MyLineItems { get; set; } = new();


        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }
        public int? BinNumber { get => _binNumber; set => SetProperty(ref _binNumber, value); }
        public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value); }
        public string CustomerEmail { get => _customerEmail; set => SetProperty(ref _customerEmail, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }

        public event Action<IDialogResult> RequestClose;
        private DelegateCommand _doneCommand;
        private string _message;
        private readonly ApiClient _apiClient;

        public DelegateCommand DoneCommand
        {
            get { return _doneCommand ??= new DelegateCommand(HandleDone); }
        }

        public override string Title => _title;

        private void HandleDone()
        {
            var result = new DialogResult(ButtonResult.OK, null);
            RequestClose?.Invoke(result);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        { }


        public AfterScanDialogViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<long?>("LineItemId", out var lineItemId))
            {
                var prams = new Dictionary<string, string> { { "LineItemId", $"{lineItemId}" } };
                var lineItems = await _apiClient.ListItemsAsync(prams);
                if (lineItems != null && lineItems.Any())
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        MyLineItems.Clear();
                        MyLineItems.Add(lineItems[0]);
                        OrderNumber = lineItems[0].OrderNumber;
                        BinNumber = lineItems[0].BinNumber;
                        CustomerName = lineItems[0].Customer;
                        CustomerEmail = lineItems[0].CustomerEmail;
                    });
                }
            }

            if (parameters.TryGetValue<string>("Title", out var title))
            {
                _title = title;
                RaisePropertyChanged(nameof(Title));
            }

            if (parameters.TryGetValue<string>("Message", out var message))
            {
                Message = message;
            }
        }
    }
}
