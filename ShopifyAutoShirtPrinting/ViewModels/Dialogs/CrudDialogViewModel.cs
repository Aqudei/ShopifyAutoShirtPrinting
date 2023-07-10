using AutoMapper;
using Prism.Commands;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Models;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    internal class CrudDialogViewModel : PageBase, IDialogAware
    {
        public override string Title => "";

        private DelegateCommand<string> _dialogCommand;
        private MyLineItem _theLineItem;
        private string _orderNumber;
        private IMapper _mapper;
        private int _quantity;
        private string notes;
        private string _name;
        private int _id;

        public DelegateCommand<string> DialogCommand
        {
            get { return _dialogCommand ??= new DelegateCommand<string>(HandleDialogCommand); }
        }

        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
        public string Notes { get => notes; set => SetProperty(ref notes, value); }
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private void HandleDialogCommand(string command)
        {
            _theLineItem.OrderNumber = OrderNumber;
            _theLineItem.Name = Name;
            _theLineItem.Quantity = Quantity;
            _theLineItem.Notes = Notes;

            var prams = new DialogParameters { { "MyLineItem", _theLineItem } };

            RequestClose.Invoke(new DialogResult(ButtonResult.OK, prams));
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
        public CrudDialogViewModel(IMapper mapper)
        {
            _mapper = mapper;
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null && parameters.TryGetValue<MyLineItem>("MyLineItem", out var myLineItem))
            {
                _theLineItem = _mapper.Map<MyLineItem>(myLineItem);
                _mapper.Map(_theLineItem, this);
            }
            else
            {
                _theLineItem = new MyLineItem
                {
                    Status = "Pending"
                };
            }
        }
    }
}
