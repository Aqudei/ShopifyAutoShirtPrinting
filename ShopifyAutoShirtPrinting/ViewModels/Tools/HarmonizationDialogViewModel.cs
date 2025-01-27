using Common.Api;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Tools
{
    internal class HarmonizationDialogViewModel : BindableBase, IDialogAware
    {
        private int _id;
        private string _description;
        private string _code;
        private DelegateCommand<string> _dialogCommand;
        private readonly ApiClient _api;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string Code { get => _code; set => SetProperty(ref _code, value); }

        public DelegateCommand<string> DialogCommand { get => _dialogCommand ??= new DelegateCommand<string>(OnDialogCommand); }

        private async void OnDialogCommand(string command)
        {
            var hsn = new Common.Models.Harmonisation.HSN
            {
                Code = _code,
                Description = _description,
                Id = _id,
            };

            if (command == "Save")
            {
                await _api.SaveHSNAsync(hsn);
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
            else if (command == "SaveAndAdd")
            {
                await _api.SaveHSNAsync(hsn);

                Code = "";
                Description = "";
                Id = 0;
            }

            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;
        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<Models.Harmonisation.HSN>("Edit", out var hsn))
            {
                Id = hsn.Id;
                Description = hsn.Description;
                Code = hsn.Code;
            }
        }

        public HarmonizationDialogViewModel(ApiClient api)
        {
            _api = api;
        }
    }
}
