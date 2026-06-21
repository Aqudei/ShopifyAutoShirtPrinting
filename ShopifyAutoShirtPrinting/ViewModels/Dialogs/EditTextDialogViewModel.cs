using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    internal class EditTextDialogViewModel : BindableBase, IDialogAware
    {
        private string _title;

        public string Title => _title;

       
        private string _text;
        private DelegateCommand<string> _dialogCommand;
        private DialogCloseListener _requestClose;

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }


        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public DelegateCommand<string> DialogCommand { get => _dialogCommand ??= new DelegateCommand<string>(OnDialogCommand); }

        DialogCloseListener IDialogAware.RequestClose => _requestClose;

        private void OnDialogCommand(string obj)
        {
            if (obj == "ok")
            {
                var dialogParameters = new DialogParameters { { "text", Text } };
                _requestClose.Invoke(dialogParameters,ButtonResult.OK);
            }
            else
            {
                _requestClose.Invoke(new DialogResult(ButtonResult.Cancel));
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            parameters.TryGetValue("title", out _title);
            parameters.TryGetValue("text", out _text);
        }
    }
}
