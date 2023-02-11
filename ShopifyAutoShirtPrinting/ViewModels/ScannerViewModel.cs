using System;
using System.Linq;
using System.Text;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class ScannerViewModel : BindableBase, IDialogAware
    {
        private string _detectedQr;
        private string _variantId;
        private string _sku;
        private string _orderNumber;
        private string _name;
        private string _lineItemId;


        public string DetectedQr
        {
            get => _detectedQr;
            set => SetProperty(ref _detectedQr, value);
        }

        public ScannerViewModel()
        {
            PropertyChanged += ScannerViewModel_PropertyChanged;
        }

        private void ScannerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(DetectedQr) == e.PropertyName)
            {
                if (!string.IsNullOrWhiteSpace(DetectedQr))
                {
                    var base64EncodedBytes = Convert.FromBase64String(DetectedQr);
                    var decodedString = Encoding.UTF8.GetString(base64EncodedBytes);

                    var parts = decodedString.Split(Environment.NewLine.ToCharArray());
                    parts = parts.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    var lineItemId = parts[0];
                    var name = parts[1];
                    var orderNumber = parts[2];
                    var quantity = parts[3];
                    var sku = parts[4];
                    var variantId = parts[5];

                    this.LineItemId = lineItemId;
                    this.Name = name;
                    this.OrderNumber = orderNumber;
                    this.Sku = sku;
                    this.VariantId = variantId;
                }
            }
        }

        public string VariantId
        {
            get => _variantId;
            set => SetProperty(ref _variantId, value);
        }

        public string Sku
        {
            get => _sku;
            set => SetProperty(ref _sku, value);
        }

        public string OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string LineItemId
        {
            get => _lineItemId;
            set => SetProperty(ref _lineItemId, value);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }


        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result));
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public string Title { get; } = "QR Code Scanner";
        public event Action<IDialogResult> RequestClose;
    }
}
