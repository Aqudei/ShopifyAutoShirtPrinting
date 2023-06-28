using AutoMapper;
using EasyNetQ;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using ZXing;
using ZXing.Common;
using Application = System.Windows.Application;
using Order = ShopifySharp.Order;
using Path = System.IO.Path;
using PrintDocument = System.Drawing.Printing.PrintDocument;

namespace ShopifyEasyShirtPrinting.ViewModels;

public class OrderProcessingViewModel : PageBase, INavigationAware
{

    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr WindowHandle);
    public const int SW_RESTORE = 9;
    public const int SW_MAXIMIZE = 3;


    private void FocusChrome()
    {
        FocusProcess("chrome");
    }


    private void FocusProcess(string procName)
    {
        var objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);

        foreach (Process proc in objProcesses)
        {
            if (proc.MainWindowTitle.Contains("ShipStation"))
            {
                IntPtr hWnd = IntPtr.Zero;
                hWnd = proc.MainWindowHandle;
                ShowWindowAsync(new HandleRef(null, hWnd), SW_MAXIMIZE);
                SetForegroundWindow(hWnd);
                break;
            }

        }
    }

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<MyLineItem> _lineItems = new();
    public ICollectionView LineItemsView { get; }
    public override string Title => "Order Items";
    private string _currentImage;
    private DelegateCommand _generateQrCommand;
    private MyLineItem _selectedLineItem;
    private readonly IDialogService _dialogService;
    private readonly OrderService _orderService;
    private readonly ShipStationApi _shipStationApi;
    private readonly IShipStationBrowserService _browserService;
    private readonly ProductVariantService _productVariantService;
    private readonly ProductImageService _productImageService;
    private readonly GlobalVariables _globalVariables;
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly LogRespository _logRespository;
    private readonly MyPrintService _myPrintService;
    private readonly IMapper _mapper;
    private readonly ILineRepository _lineRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly BinService _binService;
    private readonly DbService _dbService;
    private readonly IBus _bus;
    private readonly IEventAggregator _eventAggregator;
    private DelegateCommand _openQrScannerCommand;
    private string _searchText;
    private string _detectedQr;

    public ObservableCollection<string> ShippingLines { get; set; } = new();

    private DelegateCommand _saveQrTagsCommand;
    private DelegateCommand<string> _appplyTagCommand;
    public ObservableCollection<Log> Logs { get; set; } = new();
    public ObservableCollection<KeyValuePair<string, string>> ScanInfo { set; get; } = new();
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

    public string[] Tags => new string[] {
        "Pending",
        "Processed",
        "LabelPrinted",
        "Shipped",
        "Archived",
        "Need To Order From Supplier",
        "Have Ordered From Supplier"
    };

    public DelegateCommand<string> AppplyTagCommand
    {
        get { return _appplyTagCommand ??= new DelegateCommand<string>(HandleApplyTag); }
    }

    private DelegateCommand<MyLineItem> _openInBrowserCommand;

    public DelegateCommand<MyLineItem> OpenInBrowserCommand
    {
        get { return _openInBrowserCommand ??= new DelegateCommand<MyLineItem>(HandleOpenInBrowser); }
    }

    private void HandleOpenInBrowser(MyLineItem item)
    {
        Task.Run(() => _browserService.NavigateToOrder(item.OrderNumber));
        FocusChrome();
    }

    private void HandleApplyTag(string tag)
    {
        var selectedItems = _lineItems.Where(l => l.IsSelected);
        foreach (var selectedItem in selectedItems)
        {
            ApplyTagForLineItem(selectedItem, tag);
        }
    }

    public bool IsScanOnly { get => _isScanOnly; set => SetProperty(ref _isScanOnly, value); }

    private void ApplyTagForLineItem(MyLineItem myLineItem, string tag)
    {
        if (myLineItem.Status == tag)
        {
            return;
        }

        myLineItem.Status = tag;
        var dateNow = DateTime.Now;

        if (tag == "Pending") // Reset
        {
            myLineItem.DateModified = null;
            myLineItem.BinNumber = 0;
            myLineItem.PrintedQuantity = 0;
        }

        myLineItem.DateModified = dateNow;
        _lineRepository.Update(myLineItem);

        _logRespository.Add(new Log
        {
            ChangeDate = dateNow,
            ChangeStatus = tag,
            MyLineItemId = myLineItem.Id
        });

        _bus.PubSub.Publish(new TagUpdated
        {
            MyLineItemDatabaseId = myLineItem.Id
        });
    }

    public DelegateCommand SaveQrTagsCommand => _saveQrTagsCommand ??= new DelegateCommand(HandleSaveQrTag);


    private DelegateCommand _resetDatabaseCommand;

    public DelegateCommand ResetDatabaseCommand
    {
        get { return _resetDatabaseCommand ??= new DelegateCommand(HandleResetDatabase); }
    }

    private DelegateCommand _refreshCommand;

    public DelegateCommand RefreshCommand => _refreshCommand ??= new DelegateCommand(RefreshData);

    private async void RefreshData()
    {
        await Task.Run(FetchLineItems);
    }

    private async void HandleResetDatabase()
    {
        var prompt = await _dialogCoordinator.ShowMessageAsync(this, "Confirm Database Reset",
            "Are you sure you want to cleanup database?", MessageDialogStyle.AffirmativeAndNegative);

        if (prompt != MessageDialogResult.Affirmative)
            return;

        _dbService.ResetDatabase();

        await Task.Delay(TimeSpan.FromSeconds(3));
        await Task.Run(FetchLineItems);
    }

    private async void HandleSaveQrTag()
    {
        var dlg = new CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        var result = dlg.ShowDialog();
        if (result == CommonFileDialogResult.Ok)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Saving Qr Tags");
            foreach (var orderItem in _lineItems.Where(l => l.IsSelected))
            {
                using var combinedImage = GenerateQrImageForItem(orderItem);
                var outputName = $"{orderItem.OrderId}-{orderItem.LineItemId}-{orderItem.Name}.png";
                outputName = outputName.Replace("/", "-").Replace("\\", "-").Replace(" ", "-");
                outputName = RemoveRedundantChars(Path.Combine(dlg.FileName, outputName), "-");
                combinedImage.Save(outputName);
            }
            await progress.CloseAsync();
            Process.Start("explorer.exe", $"\"{dlg.FileName}\"");
        }
    }

    private Bitmap GenerateQrImageForItem(MyLineItem orderItem)
    {

        var qrData = new[]
                        {
                    $"{orderItem.OrderId}",
                    $"{orderItem.LineItemId}",
                    $"{orderItem.OrderNumber}"
                };

        var qrDataText = EncodeText(string.Join($"{Environment.NewLine}", qrData));

        var qrHelper = new QrHelper();

        using var qrImage = qrHelper.GenerateBitmapQr(qrDataText);
        using var textImage = qrHelper.DrawTextImage(orderItem.Name, qrImage);
        var combinedImage = qrHelper.CombineImage(qrImage, textImage);

        return combinedImage;
    }

    private string RemoveRedundantChars(string input, string chr)
    {
        var rgx = new Regex(@$"\{chr}+");
        return rgx.Replace(input, chr);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string CurrentImage
    {
        get => _currentImage;
        set => SetProperty(ref _currentImage, value);
    }

    public OrderProcessingViewModel(IDialogService dialogService, OrderService orderService, ShipStationApi shipStationApi, IShipStationBrowserService browserService,
        ProductVariantService productVariantService, ProductImageService productImageService, GlobalVariables globalVariables, IDialogCoordinator dialogCoordinator,
        IEventAggregator eventAggregator, LogRespository logRespository, IMapper mapper, ILineRepository lineRepository, MyPrintService myPrintService,
        IOrderRepository orderRepository, BinService binService, DbService dbService, IBus bus)
    {
        _dispatcher = Application.Current.Dispatcher;

        _mapper = mapper;
        _dialogService = dialogService;
        _orderService = orderService;
        _shipStationApi = shipStationApi;
        _browserService = browserService;
        _productVariantService = productVariantService;
        _productImageService = productImageService;
        _globalVariables = globalVariables;
        _dialogCoordinator = dialogCoordinator;
        _logRespository = logRespository;
        _myPrintService = myPrintService;
        _lineRepository = lineRepository;
        _orderRepository = orderRepository;
        _eventAggregator = eventAggregator;
        _binService = binService;
        _dbService = dbService;
        _bus = bus;


        LineItemsView = CollectionViewSource.GetDefaultView(_lineItems);

        Task.Run(FetchLineItems).ContinueWith(t =>
        {
            _bus.PubSub.Subscribe<TagUpdated>("tag.updated", (e) =>
            {
                if (e.MyLineItemDatabaseId == 0)
                    return;

                var dbLineItem = _lineRepository.GetById(e.MyLineItemDatabaseId);
                var currentLineItem = _lineItems.SingleOrDefault(l => l.Id == e.MyLineItemDatabaseId);

                if (dbLineItem != null && currentLineItem != null)
                {
                    if (currentLineItem.Notes != dbLineItem.Notes)
                    {
                        _dispatcher.Invoke(() => { currentLineItem.Notes = dbLineItem.Notes; });
                    }
                }
            });

            _dispatcher.Invoke(() => ShippingLines.AddRange(_lineItems.Select(l => l.Shipping).Distinct().ToList()));

        });
        PropertyChanged += OrderProcessingViewModel_PropertyChanged;
    }

    private DelegateCommand _browseQrCommand;

    public DelegateCommand BrowseQrCommand
    {
        get { return _browseQrCommand ??= new DelegateCommand(BrowseQr); }
    }

    private void BrowseQr()
    {
        var dlg = new CommonOpenFileDialog
        {
            Filters =
            {
                new CommonFileDialogFilter("Picture Files", "*.png;*.bmp;*.jpeg;*.jpg")
            }
        };
        var dialogResult = dlg.ShowDialog();

        if (dialogResult != CommonFileDialogResult.Ok)
            return;

        var qrBitmap = new Bitmap(dlg.FileName);

        var reader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            }
        };

        var result = reader.Decode(qrBitmap);

        if (result == null)
            return;
        var qrText = result.Text;
        DetectedQr = qrText + " ";
        // use qrText as needed
        // QR code not found or could not be decoded
    }

    private DelegateCommand<string> _appplyStatusFilterCommand;

    public DelegateCommand<string> AppplyStatusFilterCommand
    {
        get { return _appplyStatusFilterCommand ??= new DelegateCommand<string>(HandleFilterByStatusCommand); }
    }

    private void HandleFilterByStatusCommand(string statusTag)
    {
        LineItemsView.Filter = s =>
        {
            return (s as MyLineItem)?.Status == statusTag;
        };

    }

    public DelegateCommand ApplyNotesCommand
    {
        get => applyNotesCommand ??= new DelegateCommand(HandleApplyNotes, () => SelectedLineItem != null)
            .ObservesProperty(() => SelectedLineItem);
    }

    private void HandleApplyNotes()
    {
        SelectedLineItem.Notes = Notes;
        _lineRepository.Update(SelectedLineItem);
    }

    private DelegateCommand _clearStatusFilterCommand;

    public DelegateCommand ClearStatusFilterCommand
    {
        get { return _clearStatusFilterCommand ??= new DelegateCommand(HandleClearStatusFilter); }
    }

    private void HandleClearStatusFilter()
    {
        LineItemsView.Filter = null;
    }

    private void OrderProcessingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedLineItem):
                {
                    if (SelectedLineItem != null)
                    {
                        CurrentImage = null;
                        Task.Run(() => FetchProductImageAsync(SelectedLineItem));

                        Logs.Clear();
                        var logs = _logRespository.Find(l => l.MyLineItemId == SelectedLineItem.Id).ToList();
                        Logs.AddRange(logs);

                        Notes = SelectedLineItem.Notes;
                    }
                    break;
                }
            case nameof(SearchText):
                {
                    if (!string.IsNullOrWhiteSpace(SearchText))
                        LineItemsView.Filter = o =>
                        {
                            if (o is MyLineItem o1)
                            {
                                var sku = o1.Sku ?? "";
                                var orderNumber = o1.OrderNumber?.ToString() ?? "";
                                var shippingLines = o1.Shipping?.ToLower() ?? "";
                                return orderNumber.Contains(SearchText) || sku.Contains(SearchText) ||
                                                          o1.Name.ToLower().Contains(SearchText.ToLower()) || shippingLines.Contains(SearchText.ToLower());
                            }

                            return true;
                        };
                    else
                        LineItemsView.Filter = o => true;

                    break;
                }

            case nameof(DetectedQr):
                {
                    if (!string.IsNullOrWhiteSpace(DetectedQr) && (DetectedQr.EndsWith("\n") || DetectedQr.EndsWith("\r") ||
                                                                   DetectedQr.EndsWith(" ")))
                    {
                        var parsedQr = MyQr.Parse(DetectedQr.Trim(" \r\n\t".ToCharArray()));
                        DetectedQr = "";


                        if (parsedQr != null)
                        {
                            // QrInfo = FetchQrInfo(parsedQr);
                            ShowScanInfo(parsedQr);

                            var lineItem = _lineRepository.Get(l => l.LineItemId == parsedQr.LineItemId);

                            if (lineItem == null)
                            {
                                return;
                            }


                            if (IsScanOnly)
                            {
                                Task.Run(async () => await ShowLineItem(lineItem));
                            }
                            else
                            {
                                Task.Run(() => ProcessQrForPrinting(lineItem));
                            }

                        }
                    }

                    break;
                }
        }
    }

    private void ShowScanInfo(MyQr qr)
    {
        var lineItem = _lineItems.FirstOrDefault(l => l.OrderId == qr.OrderId && l.LineItemId == qr.LineItemId);
        if (lineItem == null)
        {
            return;
        }

        _dispatcher.Invoke(() =>
        {
            ScanInfo.Clear();
            ScanInfo.Add(new KeyValuePair<string, string>("Name", lineItem.Name));
            ScanInfo.Add(new KeyValuePair<string, string>("Quantity", lineItem.Quantity?.ToString()));
            ScanInfo.Add(new KeyValuePair<string, string>("Order#", lineItem.OrderNumber));
            ScanInfo.Add(new KeyValuePair<string, string>("SKU", lineItem.Sku));
        });

    }

    private async void ProcessQrForPrinting(MyLineItem lineItem)
    {
        try
        {
            var orderItemResult = await _myPrintService.PrintItem(lineItem);

            //await _dispatcher.InvokeAsync(() =>
            //{
            //    _mapper.Map(orderItemResult, lineItem);
            //});

            await ShowLineItem(orderItemResult);

            var lineItems = _lineItems.Where(l => l.OrderId == orderItemResult.OrderId);

            var allItemsPrinted = _myPrintService.AreAllItemsPrinted(orderItemResult.OrderId.Value, lineItems);
            if (allItemsPrinted)
                await _dispatcher.InvokeAsync(new Action(() =>
                {
                    var dlgParams = new DialogParameters
                    {
                        { "OrderId", lineItem.OrderId }
                    };

                    _dialogService.ShowDialog("LabelPrintingDialog", dlgParams, result =>
                    {
                        if (result.Result == ButtonResult.Yes)
                        {
                            foreach (var lineItem in lineItems)
                            {
                                ApplyTagForLineItem(lineItem, "LabelPrinted");
                                lineItem.BinNumber = 0;
                            }

                            _binService.EmptyBin(orderItemResult.BinNumber);

                            Task.Run(() => _browserService.NavigateToOrder(lineItem.OrderNumber));
                            FocusChrome();
                        }
                    });
                }));
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
            await _dialogCoordinator.ShowMessageAsync(this, "Error", $"{exception.Message}\n\n{exception.StackTrace}");
        }
    }

    private async Task ShowLineItem(MyLineItem orderItemResult)
    {
        await _dispatcher.InvokeAsync(new Action(() =>
        {
            SearchText = $"{orderItemResult.OrderNumber}";
            var existingOrderItem = _lineItems.FirstOrDefault(o =>
                o.OrderId == orderItemResult.OrderId && o.LineItemId == orderItemResult.LineItemId);

            if (existingOrderItem != null)
            {
                existingOrderItem.BinNumber = orderItemResult.BinNumber;
                existingOrderItem.PrintedQuantity = orderItemResult.PrintedQuantity;
                existingOrderItem.Status = orderItemResult.Status;
                SelectedLineItem = existingOrderItem;
            }
        }));
    }

    private static double MillimetersToInches(double mmValue)
    {
        const double mmPerInch = 25.4;
        const double inchesPerMm = 1.0 / mmPerInch;
        return mmValue * inchesPerMm;
    }

    private void PrintQr(Bitmap qrImage)
    {

        int paperWidth = (int)(MillimetersToInches(Properties.Settings.Default.PaperWidth) * 100);
        int paperHeight = (int)(MillimetersToInches(Properties.Settings.Default.PaperHeight) * 100);

        var paperSize = new PaperSize
        {
            Width = paperWidth,
            Height = paperHeight
        };


        var printerSettings = new PrinterSettings
        {
            PrinterName = Properties.Settings.Default.QrPrinter,
            DefaultPageSettings =
            {
                Margins = new System.Drawing.Printing.Margins(0,0,0,0),
                PaperSize = paperSize            }
        };

        // Create a new PrintDocument object
        var printDocument = new PrintDocument();
        printDocument.PrinterSettings = printerSettings;

        // Define the PrintPage event handler
        printDocument.PrintPage += (sender, e) =>
        {
            // Get the page size
            var pageSize = e.PageSettings.PrintableArea.Size;

            // Calculate the scaling factor
            var scaleX = pageSize.Width / qrImage.Width;
            var scaleY = pageSize.Height / qrImage.Height;
            var scale = Math.Min(scaleX, scaleY);

            // Calculate the position to center the image on the page
            var x = (pageSize.Width - (qrImage.Width * scale)) / 2;
            var y = (pageSize.Height - (qrImage.Height * scale)) / 2;

            // Draw the image
            e.Graphics.DrawImage(qrImage, x, y, qrImage.Width * scale, qrImage.Height * scale);

            e.HasMorePages = false;
        };

        // Start the print job
        printDocument.Print();
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
            if (item is MyLineItem orderItem)
                orderItem.IsSelected = false;
    }


    private void CheckHighlighted(IEnumerable<object> highlighted)
    {
        foreach (var item in highlighted)
            if (item is MyLineItem orderItem)
                orderItem.IsSelected = true;
    }


    private DelegateCommand _uncheckedAllCommand;

    public DelegateCommand UncheckedAllCommand
    {
        get { return _uncheckedAllCommand ??= new DelegateCommand(UncheckAll); }
    }

    private void UncheckAll()
    {
        foreach (var orderItem in _lineItems) orderItem.IsSelected = false;
    }

    private async void FetchProductImageAsync(MyLineItem myLineItem)
    {
        try
        {
            ProductImage image;

            var imagePath = Path.Combine(_globalVariables.ImagesPath, $"{myLineItem.OrderId}-{myLineItem.VariantId}");
            if (File.Exists(imagePath))
            {
                _dispatcher.Invoke(() =>
                {
                    myLineItem.ProductImage = imagePath;
                    CurrentImage = myLineItem.ProductImage;
                });
                return;
            }

            var variant = await _productVariantService.GetAsync(myLineItem.VariantId.Value);

            if (variant?.ImageId == null)
            {
                var images = await _productImageService.ListAsync(variant.ProductId.Value);
                image = images?.Items?.OrderBy(i => i.Position)?.FirstOrDefault();


                if (image != null)
                {
                    await _dispatcher.InvokeAsync(async () =>
                    {
                        myLineItem.ProductImage = await DownloadImageAsync(image.Src, imagePath); ;
                        CurrentImage = myLineItem.ProductImage;
                    });
                }
                else
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        myLineItem.ProductImage = null;
                        CurrentImage = myLineItem.ProductImage;
                    });
                }
                return;
            }

            image = await _productImageService.GetAsync(variant.ProductId.Value, variant.ImageId.Value);

            await _dispatcher.InvokeAsync(async () =>
            {
                myLineItem.ProductImage = await DownloadImageAsync(image.Src, imagePath);
                CurrentImage = myLineItem.ProductImage;
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private async Task<string> DownloadImageAsync(string imageSource, string imagePath)
    {
        using var client = new WebClient();
        await client.DownloadFileTaskAsync(new Uri(imageSource), imagePath);
        return imagePath;
    }

    private async void FetchLineItems()
    {
        var waitDialog = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Fetching orders @ page # 1...");
        waitDialog.SetIndeterminate();

        try
        {
            // Update UI
            await _dispatcher.InvokeAsync(() => _lineItems.Clear());
            var lineItems = _lineRepository.Find(x => x.Status != "Archived");
            foreach (var lineItem in lineItems)
            {
                await _dispatcher.InvokeAsync(() => _lineItems.Add(lineItem));
            }

        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        finally
        {
            await waitDialog.CloseAsync();
        }
    }

    private DelegateCommand<IEnumerable<object>> _processSelectedCommand;
    private string _notes;
    private DelegateCommand applyNotesCommand;
    private bool _isScanOnly;

    public DelegateCommand<IEnumerable<object>> ProcessSelectedCommand => _processSelectedCommand ??=
        new DelegateCommand<IEnumerable<object>>(ProcessSelectedOrders);

    private void ProcessSelectedOrders(IEnumerable<object> selectedOrders)
    {
        foreach (Order order in selectedOrders)
        {
        }
    }

    public DelegateCommand OpenQrScannerCommand
    {
        get { return _openQrScannerCommand ??= new DelegateCommand(OpenScanner); }
    }

    private void OpenScanner()
    {
        _dialogService.ShowDialog("ScannerView");
    }

    public DelegateCommand GenerateQrCommand => _generateQrCommand ??= new DelegateCommand(HandlePrintQrTags);


    private string EncodeText(string input)
    {
        // Encode
        var plainText = input;
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var encodedText = Convert.ToBase64String(plainTextBytes);
        Debug.WriteLine("Encoded text: " + encodedText);
        return encodedText;
    }

    private async void HandlePrintQrTags()
    {
        if (!_lineItems.Any(l => l.IsSelected))
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", "Please select items to generate QR for!");
            return;
        }

        var selectedItems = _lineItems.Where(l => l.IsSelected);

        foreach (var lineItem in selectedItems)
        {
            var combinedImage = GenerateQrImageForItem(lineItem);

            lineItem.Status = "Processed";
            _lineRepository.Update(lineItem);

            PrintQr(combinedImage);
        }
    }

    public MyLineItem SelectedLineItem
    {
        get => _selectedLineItem;
        set => SetProperty(ref _selectedLineItem, value);
    }

    public string DetectedQr
    {
        get => _detectedQr;
        set => SetProperty(ref _detectedQr, value);
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