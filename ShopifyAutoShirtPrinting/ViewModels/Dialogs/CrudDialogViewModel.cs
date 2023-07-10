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
        private string orderNumber;
        private string name;
        private int quantity;
        private string notes;
        private IMapper _mapper;
        private int id;
        private MyLineItem TheLineItem;

        public DelegateCommand<string> DialogCommand
        {
            get { return _dialogCommand ??= new DelegateCommand<string>(HandleDialogCommand); }
        }

        public string OrderNumber { get => orderNumber; set => SetProperty(ref orderNumber, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }
        public int Quantity { get => quantity; set => SetProperty(ref quantity, value); }
        public string Notes { get => notes; set => SetProperty(ref notes, value); }
        public int Id { get => id; set => SetProperty(ref id, value); }

        private void HandleDialogCommand(string command)
        {
            TheLineItem.OrderNumber = OrderNumber;
            TheLineItem.Name = Name;
            TheLineItem.Quantity = Quantity;
            TheLineItem.Notes = Notes;
           
            var prams = new DialogParameters { { "MyLineItem", TheLineItem } };

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
                TheLineItem = _mapper.Map<MyLineItem>(myLineItem);
                _mapper.Map(TheLineItem, this);
            }
        }
    }
}
