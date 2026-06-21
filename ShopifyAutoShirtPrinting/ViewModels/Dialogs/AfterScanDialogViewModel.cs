using AutoMapper;
using Common.Api;
using Common.Models;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
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
        private DialogCloseListener _requestClose;
        public ObservableCollection<LineItemViewModel> MyLineItems { get; set; } = new();

        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }
        public int? BinNumber { get => _binNumber; set => SetProperty(ref _binNumber, value); }
        public string CustomerName { get => _customerName; set => SetProperty(ref _customerName, value); }
        public string CustomerEmail { get => _customerEmail; set => SetProperty(ref _customerEmail, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public string Notes { get => _notes; set => SetProperty(ref _notes, value); }
        private DelegateCommand _doneCommand;
        private string _message;
        private string _notes;
        private readonly ApiClient _apiClient;
        private readonly IMapper _mapper;

        public DelegateCommand DoneCommand
        {
            get
            {

                return _doneCommand ??= new DelegateCommand(HandleDone);
            }
        }

        public override string Title => _title;

        DialogCloseListener IDialogAware.RequestClose => _requestClose;

        private void HandleDone()
        {
            var result = new DialogResult(ButtonResult.OK);
            _requestClose.Invoke(result);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        { }


        public AfterScanDialogViewModel(ApiClient apiClient, IMapper mapper)
        {
            _apiClient = apiClient;
            _mapper = mapper;
        }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<int>("Id", out var id))
            {
                var prams = new Dictionary<string, string> { { "Id", $"{id}" } };
                var lineItems = await _apiClient.ListItemsAsync(prams);

                if (lineItems != null && lineItems.Any())
                {
                    var lineItemsArray = lineItems.Select(_mapper.Map<LineItemViewModel>).ToArray();

                    var customerName = lineItemsArray.Where(l => !string.IsNullOrWhiteSpace(l.Customer)).FirstOrDefault()?.Customer;
                    var customerEmail = lineItemsArray.Where(l => !string.IsNullOrWhiteSpace(l.CustomerEmail)).FirstOrDefault()?.CustomerEmail;
                    var orderNumber = lineItemsArray.Where(l => !string.IsNullOrWhiteSpace(l.OrderNumber)).FirstOrDefault()?.OrderNumber;
                    await _dispatcher.InvokeAsync(() =>
                    {
                        MyLineItems.Clear();
                        MyLineItems.Add(lineItemsArray[0]);
                        BinNumber = lineItemsArray[0].BinNumber;
                        Notes = lineItemsArray[0].Notes;
                        OrderNumber = orderNumber;
                        CustomerName = customerName;
                        CustomerEmail = customerEmail;
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
