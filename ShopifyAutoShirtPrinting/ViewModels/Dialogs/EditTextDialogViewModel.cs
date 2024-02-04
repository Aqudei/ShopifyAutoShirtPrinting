using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
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

        public event Action<IDialogResult> RequestClose;

        private string _text;
        private DelegateCommand<string> _dialogCommand;

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

        private void OnDialogCommand(string obj)
        {
            if (obj == "ok")
            {
                var dialogParameters = new DialogParameters { { "text", Text } };
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, dialogParameters));
            }
            else
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            parameters.TryGetValue("title", out _title);
            parameters.TryGetValue("text", out _text);
        }
    }
}
