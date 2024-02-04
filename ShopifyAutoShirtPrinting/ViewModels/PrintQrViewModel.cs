using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class PrintQrViewModel : BindableBase, IDialogAware
    {

        private readonly IDialogCoordinator _dialogCoordinator;
        public ObservableCollection<string> Printers { get; set; } = new();
        public ObservableCollection<MyLineItem> SelectedOrderItems { get; set; } = new();
        private DelegateCommand _saveQrCodesCommand;
        private string _selectedPrinter;

        public DelegateCommand SaveQrCodesCommand => _saveQrCodesCommand ??= new DelegateCommand(SaveQrCodesToFile);



        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            foreach (var printer in PrinterSettings.InstalledPrinters)
            {
                Printers.Add(printer.ToString());
            }

            SelectedPrinter = Printers.FirstOrDefault();


            var selectedItems = parameters.GetValue<IEnumerable<MyLineItem>>("selectedItems");
            var orderItems = selectedItems as MyLineItem[] ?? selectedItems.ToArray();

            if (orderItems.Any())
            {
                SelectedOrderItems.AddRange(orderItems);
            }
        }

        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set => SetProperty(ref _selectedPrinter, value);
        }

        private string EncodeText(string input)
        {
            // Encode
            var plainText = input;
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var encodedText = Convert.ToBase64String(plainTextBytes);
            Debug.WriteLine("Encoded text: " + encodedText);
            return encodedText;
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

        private async void SaveQrCodesToFile()
        {
            var dlg = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            var result = dlg.ShowDialog();
            if (result != CommonFileDialogResult.Ok)
                return;


            var loading = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Generating QR Codes...");
            loading.SetIndeterminate();

            try
            {
                await Task.Run(() =>
                {
                    var barcodeWriter = new BarcodeWriterSvg();
                    foreach (var orderItem in SelectedOrderItems)
                    {
                        var qrData = new[]
                        {
                            $"{orderItem.LineItemId}",
                            $"{orderItem.Name}",
                            $"{orderItem.OrderNumber}",
                            $"{orderItem.Quantity}",
                            $"{orderItem.Sku}",
                            $"{orderItem.VariantId}",
                            $"{orderItem.VariantTitle}",
                            $"{orderItem.OrderId}"
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
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                await loading.CloseAsync();
            }
        }

        public string Title => "MyPrint QR Codes";

#pragma warning disable CS0067 // The event 'PrintQrViewModel.RequestClose' is never used
        public event Action<IDialogResult> RequestClose;
#pragma warning restore CS0067 // The event 'PrintQrViewModel.RequestClose' is never used

        public PrintQrViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
        }
    }
}
