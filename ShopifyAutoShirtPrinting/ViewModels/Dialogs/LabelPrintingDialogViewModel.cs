using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services;
using ShopifySharp;
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
        private readonly ApiClient _apiClient;

        public ObservableCollection<MyLineItem> Orders { get; set; } = new();

        public DelegateCommand<string> DialogCommand => _dialogCommand ??= new DelegateCommand<string>(OnCommand);

        private void OnCommand(string cmd)
        {
            if (cmd.ToUpper() == "YES")
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
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

        public LabelPrintingDialogViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Message", out var message))
            {
                Message = message;
            }

            if (parameters.TryGetValue<string>("OrderNumber", out var orderNumber))
            {
                Orders.Clear();
                Task.Run(async () =>
                {
                    var prams = new Dictionary<string, string>() { { "OrderNumber", $"{orderNumber}" } };
                    var lineItems = await _apiClient.ListItemsAsync(prams);

                    if (lineItems != null && lineItems.Any())
                    {
                        var customerName = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.Customer)).FirstOrDefault()?.Customer;
                        var customerEmail = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.CustomerEmail)).FirstOrDefault()?.CustomerEmail;
                        var orderNumber = lineItems.Where(l => !string.IsNullOrWhiteSpace(l.OrderNumber)).FirstOrDefault()?.OrderNumber;

                        await _dispatcher.InvokeAsync(() =>
                         {
                             Orders.AddRange(lineItems);
                             OrderNumber = orderNumber;
                             BinNumber = lineItems.FirstOrDefault()?.BinNumber;
                             CustomerName = customerName;
                             CustomerEmail = customerEmail;
                         });
                    }
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
