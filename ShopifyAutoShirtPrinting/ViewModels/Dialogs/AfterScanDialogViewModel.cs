using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class AfterScanDialogViewModel : BindableBase, IDialogAware
    {
        private string _title;
        private string _orderNumber;
        private int? _binNumber;
        private string _customerName;
        private string _customerEmail;
        private readonly ILineRepository _lineRepository;
        public ObservableCollection<MyLineItem> MyLineItems { get; set; } = new();

        public string Title => _title;

        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }
        public int? BinNumber { get => _binNumber; set => SetProperty(ref _binNumber, value); }
        public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value); }
        public string CustomerEmail { get => _customerEmail; set => SetProperty(ref _customerEmail, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }

        public event Action<IDialogResult> RequestClose;
        private DelegateCommand _doneCommand;
        private string _message;

        public DelegateCommand DoneCommand
        {
            get { return _doneCommand ??= new DelegateCommand(HandleDone); }
        }

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

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var title))
            {
                _title = title;
                RaisePropertyChanged(nameof(Title));
            }

            if (parameters.TryGetValue<string>("Message", out var message))
            {
                Message = message;
            }

            if (parameters.TryGetValue<long?>("LineItemId", out var lineItemId))
            {
                MyLineItems.Clear();

                var lineItems = _lineRepository.Get(l => l.LineItemId == lineItemId);

                if (lineItems != null)
                {
                    MyLineItems.Add(lineItems);
                    OrderNumber = lineItems.OrderNumber;
                    BinNumber = lineItems.BinNumber;
                    CustomerName = lineItems.Customer;
                    CustomerEmail = lineItems.CustomerEmail;
                }
            }
        }

        public AfterScanDialogViewModel(ILineRepository lineRepository)
        {
            _lineRepository = lineRepository;
        }
    }
}
