using Common.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class UpdateTagsDialogViewModel : BindableBase, IDialogAware
    {
        public ObservableCollection<LineItemViewModel> Items { get; set; } = new();

        public string Title => "Confirm Update";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<IEnumerable<LineItemViewModel>>("Items", out var items))
            {
                Items.Clear();
                Items.AddRange(items);
            }

            if (parameters.TryGetValue<string>("NewTag", out var newTag))
            {

            }
        }

        private DelegateCommand _okCommand;

        public DelegateCommand OkCommand => _okCommand ??= new DelegateCommand(OnOk);
        private void OnOk()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }
    }
}
