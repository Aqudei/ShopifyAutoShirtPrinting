using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Prism.Commands;
using Prism.Mvvm;
using ShopifyEasyShirtPrinting.Models;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class ScanPrintViewModel : BindableBase
    {
        private string _qrData;
        private readonly Queue<PrintItem> _printItems = new();

        private DelegateCommand _printCommand;

        public DelegateCommand PrintCommand
        {
            get { return _printCommand ??= new DelegateCommand(DoPrint); }
        }

        private void DoPrint()
        {
            // Decode
            var decodedBytes = Convert.FromBase64String(QrData);
            var decodedText = Encoding.UTF8.GetString(decodedBytes);
            Debug.WriteLine("Decoded text: " + decodedText);

            var lines = decodedText.Split($"{Environment.NewLine}".ToCharArray());
            if (lines.Length < 6)
            {
                return;
            }

            var lineItemId = lines[0].Trim();
            var variantName = lines[1].Trim();
            var orderNumber = lines[2].Trim();
            var quantity = lines[3].Trim();
            var sku = lines[4].Trim();
            var variantId = lines[5].Trim();

            var printItem = new PrintItem
            {
                LineItemId = lineItemId,
                VariantName = variantName,
                OrderNumber = orderNumber,
                Quantity = quantity,
                Sku = sku,
                VariantId = variantId
            };

            _printItems.Enqueue(printItem);
        }

        public string QrData
        {
            get => _qrData;
            set => SetProperty(ref _qrData, value);
        }

        public ScanPrintViewModel()
        {

        }
    }
}
