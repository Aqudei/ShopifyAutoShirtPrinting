using AutoMapper;
using Prism.Commands;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Models;
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

            var resultLineItem = _mapper.Map<MyLineItem>(this);
            var prams = new DialogParameters { { "MyLineItem", resultLineItem } };

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
                _mapper.Map(myLineItem, this);
            }
        }
    }
}
