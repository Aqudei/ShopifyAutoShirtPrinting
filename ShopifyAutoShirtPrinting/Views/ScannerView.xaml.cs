using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using FastExpressionCompiler.LightExpression;
using ShopifyEasyShirtPrinting.ViewModels;
using ZXing;

namespace ShopifyEasyShirtPrinting.Views
{
    /// <summary>
    /// Interaction logic for ScannerView.xaml
    /// </summary>
    public partial class ScannerView : UserControl
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private readonly Dispatcher _dispatcher;
        private volatile bool _isImageProcessing;


        public ScannerView()
        {


            InitializeComponent();
            GetVideoDevices();


            Loaded += ScannerView_Loaded;

            Unloaded += ScannerView_Unloaded;

            _dispatcher = Application.Current.Dispatcher;
        }

        private void ScannerView_Unloaded(object sender, RoutedEventArgs e)
        {
            _videoSource.NewFrame -= NewFrameHandler;
            _videoSource.SignalToStop();
        }

        private void ScannerView_Loaded(object sender, RoutedEventArgs e)
        {
            _videoSource = new VideoCaptureDevice(_videoDevices[ComboBoxDevices.SelectedIndex].MonikerString);

            _videoSource.NewFrame += NewFrameHandler;
            _videoSource.Start();
        }


        private BitmapImage BitmapImageConverter(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private void NewFrameHandler(object sender, NewFrameEventArgs eventArgs)
        {
            //if (!_isImageProcessing)
            //{
            //    ProcessImage(eventArgs.Frame);
            //    return;
            //}

            ProcessImage(eventArgs.Frame);

            _dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                var bitmapImage = BitmapImageConverter(eventArgs.Frame);
                ImageVideo.Source = bitmapImage;
            }));
        }

        private void ProcessImage(Bitmap barcodeBitmap)
        {

            _isImageProcessing = true;
            // create a barcode reader instance
            IBarcodeReader reader = new BarcodeReader();

            // detect and decode the barcode inside the bitmap
            var result = reader.Decode(barcodeBitmap);
            // do something with the result
            if (result != null)
            {
                Debug.WriteLine(result.Text);


                _dispatcher.BeginInvoke(new Action(() => ((ScannerViewModel)DataContext).DetectedQr = result.Text));

            }
            _isImageProcessing = false;
        }

        private void GetVideoDevices()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count == 0)
            {
                MessageBox.Show("No video sources found");
                return;
            }

            foreach (FilterInfo device in _videoDevices)
            {
                ComboBoxDevices.Items.Add(device.Name);
            }
            ComboBoxDevices.SelectedIndex = 0;
        }

    }
}
