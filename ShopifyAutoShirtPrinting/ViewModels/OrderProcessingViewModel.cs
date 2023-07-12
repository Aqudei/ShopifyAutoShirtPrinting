using AutoMapper;
using DYMOPrintingSupportLib;
using ImTools;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Extensions;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using ShopifyEasyShirtPrinting.ViewModels.Dialogs;
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
using System.Web.UI.WebControls;
using System.Windows.Controls;
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
    private readonly MyPrintService _myPrintService;
    private readonly IMapper _mapper;
    private readonly BinService _binService;
    private readonly DbService _dbService;
    private readonly ApiClient _apiClient;
    private readonly MessageBus _messageBus;
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
        "Issue Needs Resolving",
        "Need To Order From Supplier",
        "Have Ordered From Supplier"
    };
    private string _selectedTagFilter;

    public string SelectedTagFilter
    {
        get { return _selectedTagFilter; }
        set { SetProperty(ref _selectedTagFilter, value); }
    }

    private DelegateCommand<string> _printCommand;

    public DelegateCommand<string> PrintCommand
    {
        get { return _printCommand ??= new DelegateCommand<string>(HandlePrintCommand, arg => SelectedLineItem != null).ObservesProperty(() => SelectedLineItem); }
    }

    private async void HandlePrintCommand(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return;
        switch (arg)
        {
            case "+":
                {
                    await ShowScanInfoAsync(SelectedLineItem);
                    await ProcessItemForPrintingAsync(SelectedLineItem);
                    break;
                }
            case "-":
                {
                    var processingResult = await _apiClient.UndoPrintAsync(SelectedLineItem.Id);   
                    break;
                }
            default:
                break;
        }
    }

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

    private async void HandleApplyTag(string tag)
    {
        var selectedItems = _lineItems.Where(l => l.IsSelected);
        var updatedItems = new List<int>();

        foreach (var selectedItem in selectedItems)
        {
            await ApplyTagForLineItem(selectedItem, tag);
            updatedItems.Add(selectedItem.Id);
        }
    }

    public bool IsScanOnly { get => _isScanOnly; set => SetProperty(ref _isScanOnly, value); }

    private async Task ApplyTagForLineItem(MyLineItem myLineItem, string tag)
    {
        if (myLineItem.Status == tag)
        {
            return;
        }

        if (myLineItem.Id == 0)
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

        var updatedLineItem = await _apiClient.UpdateLineItemAsync(myLineItem);
        var newLog = await _apiClient.AddNewLogAsync(new Log
        {
            ChangeDate = dateNow,
            ChangeStatus = tag,
            MyLineItemId = myLineItem.Id
        });
    }

    public DelegateCommand SaveQrTagsCommand => _saveQrTagsCommand ??= new DelegateCommand(HandleSaveQrTag, () => TotalSelected > 0)
        .ObservesProperty(() => TotalSelected);

    private DelegateCommand _resetDatabaseCommand;

    public DelegateCommand ResetDatabaseCommand
    {
        get { return _resetDatabaseCommand ??= new DelegateCommand(HandleResetDatabase); }
    }

    private DelegateCommand _refreshCommand;

    public DelegateCommand RefreshCommand => _refreshCommand ??= new DelegateCommand(RefreshData);


    private DelegateCommand<string> _crudCommand;

    public DelegateCommand<string> CrudCommand
    {
        get { return _crudCommand ??= new DelegateCommand<string>(HandleCrudCommand); }
    }

    private void HandleCrudCommand(string crudCommand)
    {
        switch (crudCommand.ToUpper())
        {
            case "CREATE":
                _dialogService.ShowDialog("CrudDialog", callback: async (r) =>
                {
                    if (r.Parameters.TryGetValue<MyLineItem>("MyLineItem", out var myLineItem))
                    {
                        var createdLineItem = await _apiClient.CreateLineItemAsync(myLineItem);
                    }
                });
                break;
            case "UPDATE":
                if (SelectedLineItem == null) return;

                var prams = new DialogParameters { { "MyLineItem", SelectedLineItem } };
                _dialogService.ShowDialog("CrudDialog", prams, async (r) =>
                {
                    if (r.Parameters.TryGetValue<MyLineItem>("MyLineItem", out var myLineItem))
                    {
                        var lineItem = await _apiClient.UpdateLineItemAsync(myLineItem);
                    }
                });
                break;
            default:
                break;
        }
    }

    public int TotalDisplayed
    {
        get
        {
            return LineItemsView.Cast<MyLineItem>().Count();
        }
    }

    public int TotalItems
    {
        get
        {
            return LineItemsView.SourceCollection.Cast<MyLineItem>().Count();
        }
    }
    public int TotalSelected
    {
        get
        {
            return _lineItems.Where(l => l.IsSelected).Count();
        }
    }

    private async void RefreshData()
    {
        await FetchLineItems();
    }

    private async void HandleResetDatabase()
    {
        var prompt = await _dialogCoordinator.ShowMessageAsync(this, "Confirm Database Reset",
            "Are you sure you want to cleanup database?", MessageDialogStyle.AffirmativeAndNegative);

        if (prompt != MessageDialogResult.Affirmative)
            return;

        await _apiClient.ResetDatabase();
        await Task.Delay(TimeSpan.FromSeconds(3));
        await FetchLineItems();
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

        var hasNotes = !string.IsNullOrWhiteSpace(orderItem.Notes);

        var qrDataText = EncodeText(string.Join($"{Environment.NewLine}", qrData));

        var qrHelper = new QrHelper();

        using var qrImage = qrHelper.GenerateBitmapQr(qrDataText);
        using var textImage = qrHelper.DrawTextImage(orderItem.Name, qrImage, orderItem.OrderNumber, hasNotes);
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
        IEventAggregator eventAggregator, IMapper mapper, MyPrintService myPrintService,
        BinService binService, DbService dbService, ApiClient apiClient, MessageBus messageBus)
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
        _myPrintService = myPrintService;
        _eventAggregator = eventAggregator;
        _binService = binService;
        _dbService = dbService;
        _apiClient = apiClient;
        _messageBus = messageBus;
        LineItemsView = CollectionViewSource.GetDefaultView(_lineItems);
        LineItemsView.SortDescriptions.Add(new SortDescription("OrderNumber", ListSortDirection.Descending));

        // _dispatcher.Invoke(() => ShippingLines.AddRange(_lineItems.Select(l => l.Shipping).Distinct().ToList()));
        PropertyChanged += OrderProcessingViewModel_PropertyChanged;
        _messageBus.ItemsUpdated += _messageBus_ItemsUpdated;
        _messageBus.ItemsAdded += _messageBus_ItemsAdded;

        LineItemsView.CollectionChanged += LineItemsView_CollectionChanged;

        Task.Run(FetchLineItems);
    }

    private void LineItemsView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateMasterCheckBoxState();
        RaisePropertyChanged(nameof(TotalDisplayed));
        RaisePropertyChanged(nameof(TotalItems));
    }

    private void UpdateMasterCheckBoxState()
    {
        var allSelected = LineItemsView.Cast<MyLineItem>().All(x => x.IsSelected);
        var noneSelected = LineItemsView.Cast<MyLineItem>().All(x => !x.IsSelected);
        if (allSelected)
        {
            MasterCheckBoxState = true;
        }
        else if (noneSelected)
        {
            MasterCheckBoxState = false;
        }
        else
        {
            MasterCheckBoxState = null;
        }
    }

    private async void _messageBus_ItemsAdded(object sender, int[] ids)
    {
        Debug.WriteLine($"@_messageBus_ItemsAdded() -> {ids}");
        var items = await _apiClient.ListLineItemsAsync(ids);
        foreach (var item in items)
        {
            var lineItem = _lineItems.FirstOrDefault(x => x.Id == item.Id);
            if (lineItem == null)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    item.PropertyChanged += LineItem_PropertyChanged;
                    _lineItems.Add(item);
                });
            }
        }
    }

    private async void _messageBus_ItemsUpdated(object sender, int[] ids)
    {
        Debug.WriteLine($"@_messageBus_ItemsUpdated() -> {ids}");

        var items = await _apiClient.ListLineItemsAsync(ids);
        foreach (var item in items)
        {
            var lineItem = _lineItems.FirstOrDefault(x => x.Id == item.Id);
            if (lineItem != null)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    _mapper.Map(item, lineItem);
                });
            }
        }
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

    private void HandleTagFilter(string statusTag)
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

    private async void HandleApplyNotes()
    {
        SelectedLineItem.Notes = Notes;
        await _apiClient.UpdateLineItemAsync(SelectedLineItem);
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

    private async void OrderProcessingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedTagFilter):
                {
                    HandleTagFilter(SelectedTagFilter);
                    break;
                }
            case nameof(SelectedLineItem):
                {
                    if (SelectedLineItem != null)
                    {
                        CurrentImage = null;
                        await FetchProductImageAsync(SelectedLineItem);

                        Logs.Clear();
                        var logs = await _apiClient.ListLogsAsync(SelectedLineItem.Id);
                        if (logs != null)
                        {
                            await _dispatcher.InvokeAsync(() => Logs.AddRange(logs));
                        }

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

                        await _dispatcher.InvokeAsync(() => DetectedQr = "");


                        if (parsedQr != null)
                        {
                            // QrInfo = FetchQrInfo(parsedQr);
                            var lineItem = await _apiClient.GetItemByLineItemIdAsync(parsedQr.LineItemId.Value);
                            if (lineItem == null)
                            {
                                return;
                            }
                            await ShowScanInfoAsync(lineItem);
                            if (IsScanOnly)
                            {
                                await ActivateLineItemInView(lineItem);
                            }
                            else
                            {
                                await ProcessItemForPrintingAsync(lineItem);
                            }

                        }
                    }

                    break;
                }
        }
    }

    private async Task ShowScanInfoAsync(MyLineItem lineItem)
    {
        await _dispatcher.InvokeAsync(() =>
        {
            ScanInfo.Clear();
            ScanInfo.Add(new KeyValuePair<string, string>("Name", lineItem.Name));
            ScanInfo.Add(new KeyValuePair<string, string>("Quantity", lineItem.Quantity?.ToString()));
            ScanInfo.Add(new KeyValuePair<string, string>("Order#", lineItem.OrderNumber));
            ScanInfo.Add(new KeyValuePair<string, string>("SKU", lineItem.Sku));
        });

    }

    private async Task ProcessItemForPrintingAsync(MyLineItem lineItem)
    {
        try
        {

            _myPrintService.PrintItem(lineItem);
            var processingItemResult = await _apiClient.ProcessItem(lineItem.Id);


            await ActivateLineItemInView(processingItemResult.LineItem);

            var lineItems = await _apiClient.ListItemsAsync(new Dictionary<string, string> { { "OrderId", $"{lineItem.OrderId}" } });

            if (processingItemResult.AllItemsPrinted)
            {
                if (lineItem.Shipping == "Sydney Warehouse / Studio")
                {
                    _dialogService.ShowAfterScanDialog("For Pickup", "Pick up order is ready.", lineItem.LineItemId, async result =>
                    {
                        foreach (var lineItem in lineItems)
                        {
                            await ApplyTagForLineItem(lineItem, "LabelPrinted");
                        }
                    });
                }
                else
                    await _dispatcher.InvokeAsync(() =>
                    {
                        _dialogService.ShowLabelPrintingDialog(lineItem.OrderId.Value, "Ready to Print Shipping label?", async result =>
                        {
                            if (result.Result == ButtonResult.Yes)
                            {
                                foreach (var lineItem in lineItems)
                                {
                                    await ApplyTagForLineItem(lineItem, "LabelPrinted");
                                }

                                await _binService.EmptyBinAsync(processingItemResult.LineItem.BinNumber);
                                _browserService.NavigateToOrder(lineItem.OrderNumber);
                                FocusChrome();
                            }
                        });
                    });
            }
            else
            {
                var prams = new Dictionary<string, string> { { "OrderId", $"{lineItem.OrderId}" } };
                var orderInfo = _apiClient.GetOrderInfoBy(prams);

                if (lineItems.Where(l => l.BinNumber > 0 && l.OrderId == lineItem.OrderId).Sum(x => x.PrintedQuantity) > 1)
                {
                    await _dispatcher.InvokeAsync(new Action(() =>
                    {
                        var dlgParams = new DialogParameters
                                        {
                                            { "LineItemId", lineItem.LineItemId },
                                            { "Message", "Added item to exisiting bin:" },
                                            { "Title", "BIN NUMBER ASSIGNED" }
                                        };

                        _dialogService.ShowDialog("AfterScanDialog", dlgParams, result => { });

                    }));
                }
                else
                {
                    await _dispatcher.InvokeAsync(new Action(() =>
                    {
                        var dlgParams = new DialogParameters
                                        {
                                            { "LineItemId", lineItem.LineItemId },
                                            { "Message", "This item goes into new bin:" },
                                            { "Title", "NEW BIN CREATED" }
                                        };

                        _dialogService.ShowDialog("AfterScanDialog", dlgParams, result => { });

                    }));
                }
            }

            //if (!_globalVariables.IsOnLocalMachine)
            //    _bus.PubSub.Publish(new ItemsUpdated
            //    {
            //        MyLineItemDatabaseIds = new int[] { lineItem.Id }
            //    });
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
            await _dialogCoordinator.ShowMessageAsync(this, "Error", $"{exception.Message}\n\n{exception.StackTrace}");
        }
    }

    private async Task ActivateLineItemInView(MyLineItem orderItemResult)
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

    public DelegateCommand<bool?> CheckBoxCommand
    {
        get => checkBoxCommand ??= new DelegateCommand<bool?>(HandleCheckBoxCommand);
    }

    public bool? MasterCheckBoxState { get => _masterCheckBoxState; set => SetProperty(ref _masterCheckBoxState, value); }

    private void HandleCheckBoxCommand(bool? checkBoxState)
    {
        if (checkBoxState.HasValue)
            foreach (var orderItem in LineItemsView.Cast<MyLineItem>())
            {
                orderItem.IsSelected = checkBoxState.Value;
            }

    }

    private async Task FetchProductImageAsync(MyLineItem myLineItem)
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

    private async Task FetchLineItems()
    {
        var waitDialog = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Fetching orders @ page # 1...");
        waitDialog.SetIndeterminate();

        try
        {
            // Update UI
            await _dispatcher.InvokeAsync(() => _lineItems.Clear());
            var lineItems = await _apiClient.ListItemsAsync();

            if (lineItems == null)
            {
                return;
            }

            foreach (var lineItem in lineItems)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    lineItem.PropertyChanged += LineItem_PropertyChanged;
                    _lineItems.Add(lineItem);
                });
            }

            await _dispatcher.InvokeAsync(() => RaisePropertyChanged(nameof(TotalItems)));
        }
        catch (Exception e)
        {
            Debug.WriteLine($"{e}\n{e.StackTrace}");
        }
        finally
        {
            await waitDialog.CloseAsync();
        }
    }

    private void LineItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        MyLineItem myLineItem;
        if (e.PropertyName == nameof(myLineItem.IsSelected))
        {
            RaisePropertyChanged(nameof(TotalSelected));
            UpdateMasterCheckBoxState();
        }
    }

    private DelegateCommand<IEnumerable<object>> _processSelectedCommand;
    private string _notes;
    private DelegateCommand applyNotesCommand;
    private bool _isScanOnly;
    private DelegateCommand<bool?> checkBoxCommand;
    private bool? _masterCheckBoxState;

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

    public DelegateCommand GenerateQrCommand => _generateQrCommand ??= new DelegateCommand(HandlePrintQrTags, () => TotalSelected > 0)
        .ObservesProperty(() => TotalSelected);

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
        var updatedIds = new List<int>();

        foreach (var lineItem in selectedItems)
        {
            var combinedImage = GenerateQrImageForItem(lineItem);

            PrintQr(combinedImage);

            lineItem.Status = "Processed";
            await _apiClient.UpdateLineItemAsync(lineItem);
            updatedIds.Add(lineItem.Id);
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

// 170.64.158.123