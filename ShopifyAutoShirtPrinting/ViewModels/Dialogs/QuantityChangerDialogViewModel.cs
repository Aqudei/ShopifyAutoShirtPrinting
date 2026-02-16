using AutoMapper;
using Common.Models;
using ImTools;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class QuantityChangerDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Change Printed Quantity";

        public event Action<IDialogResult> RequestClose;
        public MyLineItem LineItem { get; set; } = new();
        private DelegateCommand<string> _dialogCommand;
        private string _message;
        private readonly IMapper _mapper;

        public string Message
        {
            get => _message; set => SetProperty(ref _message, value);
        }


        public DelegateCommand<string> DialogCommand => _dialogCommand ??= new DelegateCommand<string>(HandleDialogCommand);

        private void HandleDialogCommand(string cmd)
        {
            if (cmd.ToLower() == "save")
            {
                if (LineItem.PrintedQuantity > LineItem.Quantity || LineItem.PrintedQuantity < 0)
                {
                    Message = "<PrintedQuantity> must be within the range of ordered Quantity (minimum 0).";
                    DelayedClear();
                    return;
                }

                var data = new DialogParameters() {
                    { "LineItem", LineItem  }
                };

                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, data));
            }

            if (cmd.ToLower() == "close")
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            }
        }

        private void DelayedClear()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (sender, e) =>
            {
                Message = "";
                timer.Stop();
            };

            timer.Start();
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
        
        public QuantityChangerDialogViewModel(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<MyLineItem>("LineItem", out var lineItem))
            {
                LineItem = new MyLineItem();
                _mapper.Map(lineItem, LineItem);
            }
        }
    }
}
