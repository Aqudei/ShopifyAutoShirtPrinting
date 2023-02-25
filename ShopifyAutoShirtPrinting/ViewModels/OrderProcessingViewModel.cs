using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Models;
using ShopifySharp;
using ZXing;
using ZXing.Common;
using Brushes = System.Drawing.Brushes;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class OrderProcessingViewModel : BindableBase, INavigationAware
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

        public OrderProcessingViewModel(IDialogService dialogService, OrderService orderService,
            ProductVariantService productVariantService, ProductImageService productImageService,
            IDialogCoordinator dialogCoordinator)
        {
            _dialogService = dialogService;
            _orderService = orderService;
            _productVariantService = productVariantService;
            _productImageService = productImageService;
            _dialogCoordinator = dialogCoordinator;

            _currentDispatcher = Application.Current.Dispatcher;
            OrdersView = CollectionViewSource.GetDefaultView(_lineItems);
            Task.Run(FetchOderItems).ContinueWith(FetchProductImages);
        }


        private DelegateCommand<IEnumerable<object>> _checkHighlightedCommand;

        public DelegateCommand<IEnumerable<object>> CheckHighlightedCommand
        {
            get { return _checkHighlightedCommand ??= new DelegateCommand<IEnumerable<object>>(CheckHighlighted); }
        }


        private DelegateCommand<IEnumerable<object>> _uncheckHighlightedCommand;

        public DelegateCommand<IEnumerable<object>> UncheckHighlightedCommand
        {
            get { return _uncheckHighlightedCommand ??= new DelegateCommand<IEnumerable<object>>(UncheckHighlighted); }
        }

        private void UncheckHighlighted(IEnumerable<object> highlighted)
        {
            foreach (var item in highlighted)
            {
                if (item is OrderItem orderItem)
                {
                    orderItem.IsSelected = false;
                }
            }
        }


        private void CheckHighlighted(IEnumerable<object> highlighted)
        {
            foreach (var item in highlighted)
            {
                if (item is OrderItem orderItem)
                {
                    orderItem.IsSelected = true;
                }
            }
        }


        private DelegateCommand _uncheckedAllCommand;

        public DelegateCommand UncheckedAllCommand
        {
            get { return _uncheckedAllCommand ??= new DelegateCommand(UncheckAll); }
        }

        private void UncheckAll()
        {
            foreach (var orderItem in _lineItems)
            {
                orderItem.IsSelected = false;
            }
        }

        private async void FetchProductImages(Task previous)
        {
            foreach (var orderItem in _lineItems)
            {
                var variant = await _productVariantService.GetAsync(orderItem.VariantId.Value);
                if (variant.ImageId == null) continue;
                var image = await _productImageService.GetAsync(variant.ProductId.Value, variant.ImageId.Value);

                orderItem.ProductImage = DownloadImage(image.Src, orderItem);
            }
        }

        private string DownloadImage(string imageSource, OrderItem orderItem)
        {
            var dataDir = Path.Combine(Path.GetTempPath(), "ShopifyImages");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            var tempFile = Path.Combine(dataDir, orderItem.VariantId.ToString());
            if (File.Exists(tempFile)) return tempFile;
            using var client = new WebClient();
            client.DownloadFile(imageSource, tempFile);

            return tempFile;
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
        private readonly IDialogCoordinator _dialogCoordinator;


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


        private async void GenerateQr()
        {
            if (!_lineItems.Any(l => l.IsSelected))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", "Please select items to generate QR for!");
                return;
            }

            var dlgParams = new DialogParameters { { "selectedItems", _lineItems.Where(i => i.IsSelected) } };

            _dialogService.ShowDialog("PrintQrView", dlgParams, result =>
            {

            });
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

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Debug.WriteLine(navigationContext.Parameters);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Debug.WriteLine(navigationContext.Parameters);

        }
    }
}
