using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifySharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    internal class LabelPrintingDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _dialogCommand;
        private int? _binNumber;
        private string _orderNumber;
        private string _customerName;
        private string _customerEmail;
        private readonly ILineRepository _lineRepository;

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

        public LabelPrintingDialogViewModel(ILineRepository lineRepository)
        {
            _lineRepository = lineRepository;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            parameters.TryGetValue<long?>("OrderId", out var orderId);
            if (orderId.HasValue)
            {

                Orders.Clear();

                var lineItems = _lineRepository.Find(l => l.OrderId == orderId).ToList();

                if (lineItems.Any())
                {
                    Orders.AddRange(lineItems);
                    OrderNumber = lineItems.FirstOrDefault()?.OrderNumber;
                    BinNumber = lineItems.FirstOrDefault()?.BinNumber;
                    CustomerName = lineItems.FirstOrDefault()?.Customer;
                    CustomerEmail = lineItems.FirstOrDefault()?.CustomerEmail;
                }
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

        public string Title { get; } = "Print Shipment Label";
        public event Action<IDialogResult> RequestClose;
    }
}
