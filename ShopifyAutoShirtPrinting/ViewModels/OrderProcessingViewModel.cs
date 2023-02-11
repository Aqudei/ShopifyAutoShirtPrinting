using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Models;
using ShopifySharp;
using ZXing;
using ZXing.Common;
using Brushes = System.Drawing.Brushes;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class OrderProcessingViewModel : BindableBase
    {
        private readonly ObservableCollection<OrderItem> _lineItems = new();

        public ICollectionView OrdersView { get; }

        private readonly Dispatcher _currentDispatcher;
        public string Title { get; set; } = "Order Processing";

        private string _currentImage;

        public string CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public OrderProcessingViewModel(IDialogService dialogService, OrderService orderService, ProductVariantService productVariantService, ProductImageService productImageService)
        {
            _dialogService = dialogService;
            _orderService = orderService;
            _productVariantService = productVariantService;
            _productImageService = productImageService;

            _currentDispatcher = Application.Current.Dispatcher;
            OrdersView = CollectionViewSource.GetDefaultView(_lineItems);
            Task.Run(FetchOderItems).ContinueWith(FetchProductImages);
        }

        private async void FetchProductImages(Task previous)
        {
            foreach (var orderItem in _lineItems)
            {
                var variant = await _productVariantService.GetAsync(orderItem.VariantId.Value);
                if (variant.ImageId != null)
                {
                    var image = await _productImageService.GetAsync(variant.ProductId.Value, variant.ImageId.Value);
                    orderItem.ProductImage = image.Src;
                }
            }
        }

        private async Task FetchOderItems()
        {
            var orders = await _orderService.ListAsync();
            await _currentDispatcher.BeginInvoke(new Action(() => _lineItems.Clear()));

            foreach (var order in orders.Items)
            {
                foreach (var orderLineItem in order.LineItems)
                {
                    var orderItem = new OrderItem
                    {
                        IsSelected = false,
                        Name = orderLineItem.Name,
                        OrderNumber = order.OrderNumber,
                        Sku = orderLineItem.SKU,
                        VariantId = orderLineItem.VariantId,
                        VariantTitle = orderLineItem.VariantTitle,
                        LineItemId = orderLineItem.Id,
                        Quantity = orderLineItem.Quantity,
                        FulfillmentStatus = orderLineItem.FulfillmentStatus,
                        FinancialStatus = order.FinancialStatus,
                        Customer = $"{order.Customer.FirstName} {order.Customer.LastName}",
                        CustomerEmail = order.Customer.Email,
                    };

                    await _currentDispatcher.BeginInvoke(new Action(() => _lineItems.Add(orderItem)));
                }
            }
        }

        private DelegateCommand<IEnumerable<object>> _processSelectedCommand;

        public DelegateCommand<IEnumerable<object>> ProcessSelectedCommand => _processSelectedCommand ??= new DelegateCommand<IEnumerable<object>>(ProcessSelectedOrders);

        private void ProcessSelectedOrders(IEnumerable<object> selectedOrders)
        {
            foreach (Order order in selectedOrders)
            {

            }
        }

        private DelegateCommand _generateQrCommand;
        private OrderItem _selectedVariant;
        private readonly IDialogService _dialogService;
        private readonly OrderService _orderService;
        private readonly ProductVariantService _productVariantService;
        private readonly ProductImageService _productImageService;


        private DelegateCommand _openQrScannerCommand;

        public DelegateCommand OpenQrScannerCommand
        {
            get { return _openQrScannerCommand ??= new DelegateCommand(OpenScanner); }
        }

        private void OpenScanner()
        {
            _dialogService.ShowDialog("ScannerView");
        }

        public DelegateCommand GenerateQrCommand => _generateQrCommand ??= new DelegateCommand(GenerateQr);

        private string EncodeText(string input)
        {
            // Encode
            var plainText = input;
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var encodedText = Convert.ToBase64String(plainTextBytes);
            Debug.WriteLine("Encoded text: " + encodedText);
            return encodedText;
        }

        private void GenerateQr()
        {
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            var result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok != true)
                return;

            Task.Run(() =>
            {
                var barcodeWriter = new BarcodeWriterSvg();
                var selectedItems = _lineItems.Where(l => l.IsSelected);
                foreach (var orderItem in selectedItems)
                {
                    var qrData = new[]
                    {
                        $"{orderItem.LineItemId}",
                        $"{orderItem.Name}",
                        $"{orderItem.OrderNumber}",
                        $"{orderItem.Quantity}",
                        $"{orderItem.Sku}",
                        $"{orderItem.VariantId}"
                    };

                    var qrDataText = EncodeText(string.Join(Environment.NewLine, qrData));

                    var outputName = Path.Combine(dlg.FileName, orderItem.VariantTitle.Replace("/", "-")
                        .Replace(" ", "") + $"-{orderItem.OrderNumber}-{orderItem.LineItemId}" + ".png");

                    var bitmapQr = GenerateBitmapQr(qrDataText, orderItem.Name);

                    SaveQrToFile(bitmapQr, outputName);
                }

                Process.Start("explorer.exe", $"\"{dlg.FileName}\"");
            });
        }

        private static void SaveQrToFile(Bitmap qrInput, string output)
        {
            // Save Bitmap to PNG
            var qrCodeImage = new BitmapImage();
            using var stream = new System.IO.FileStream(output, FileMode.Create);
            qrInput.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            qrCodeImage.BeginInit();
            qrCodeImage.CacheOption = BitmapCacheOption.OnLoad;
            qrCodeImage.StreamSource = stream;
            qrCodeImage.EndInit();
            stream.Flush();
        }

        private static Bitmap GenerateBitmapQr(string data, string bottomText)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = 600,
                    Height = 600,
                    Margin = 5,
                }
            };

            var qrCode = barcodeWriter.Write(data);
            // Add text at the bottom of the QR code image

            using var graphics = Graphics.FromImage(qrCode);
            var measure = graphics.MeasureString(bottomText, new Font("Arial", 8), qrCode.Size);
            graphics.DrawString(bottomText, new Font("Arial", 8), Brushes.Black,
                new PointF(0 + (qrCode.Width - measure.Width) / 2, qrCode.Height - 30));

            return qrCode;
        }


        public OrderItem SelectedVariant
        {
            get => _selectedVariant;
            set
            {
                SetProperty(ref _selectedVariant, value);
                CurrentImage = _selectedVariant.ProductImage;
            }
        }
    }
}
