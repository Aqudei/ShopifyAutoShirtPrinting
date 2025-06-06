using AutoMapper;
using Common.Api;
using Common.Models;
using ImTools;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Extensions;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services;
using ShopifyEasyShirtPrinting.Services.ShipStation;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using ZXing;
using ZXing.Common;
using Application = System.Windows.Application;
using Path = System.IO.Path;
using PrintDocument = System.Drawing.Printing.PrintDocument;

namespace ShopifyEasyShirtPrinting.ViewModels;

public class ArchivedViewModel : PageBase, INavigationAware
{

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<LineItemViewModel> _lineItems = new();
    public ICollectionView LineItemsView { get; }
    public override string Title => "Order Items";
    private string _currentImage;
    private DelegateCommand _generateQrCommand;
    private LineItemViewModel _selectedLineItem;
    private readonly IDialogService _dialogService;

    private readonly SessionVariables _globalVariables;
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly MyPrintService _myPrintService;
    private readonly IMapper _mapper;
    private readonly BinService _binService;
    private readonly ApiClient _apiClient;
    private readonly IMessageBus _messageBus;
    private readonly IEventAggregator _eventAggregator;
    private DelegateCommand _openQrScannerCommand;
    private string _searchText;
    private string _detectedQr;

    public ObservableCollection<string> ShippingLines { get; set; } = new();

    private DelegateCommand<string> _applyTagCommand;
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

    public string[] TagsFilter => new string[] {
        "Pending",
        "Processed",
        "LabelPrinted",
        "Shipped",
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
                    if (SelectedLineItem.PrintedQuantity == 0) return;
                    var processingResult = await _apiClient.UndoPrintAsync(SelectedLineItem.Id);
                    break;
                }
            default:
                break;
        }
    }

    public DelegateCommand<string> AppplyTagCommand
    {
        get { return _applyTagCommand ??= new DelegateCommand<string>(HandleApplyTag); }
    }

    private DelegateCommand<LineItemViewModel> _openInBrowserCommand;

    public DelegateCommand<LineItemViewModel> OpenInBrowserCommand
    {
        get { return _openInBrowserCommand ??= new DelegateCommand<LineItemViewModel>(HandleOpenInBrowser); }
    }

    private void HandleOpenInBrowser(LineItemViewModel item)
    {
        //Task.Run(async () =>
        //{
        //    var result = await _browserService.NavigateToOrderAsync(item.OrderNumber);
        //    if (!result)
        //    {
        //        WindowHelper.FocusSelf();
        //        await _dialogCoordinator.ShowMessageAsync(this, "Order Not Found!", $"Cannot find Order #{item.OrderNumber} in ShipStation!\nYou may need to refresh/reload your store in Shipstaion.");
        //    }

        //});
        //WindowHelper.FocusChrome();
    }

    private async void HandleApplyTag(string tag)
    {
        var selectedItems = _lineItems.Where(l => l.IsSelected).ToArray();

        var sb = new StringBuilder();
        foreach (var item in selectedItems)
        {
            sb.AppendLine($"#{item.OrderNumber} - {item.Name}");
        }

        var dialog = await _dialogCoordinator.ShowMessageAsync(this, "Update tags confirmation", $"Are you sure you want to update the tags of the following items?\n\n{sb}", MessageDialogStyle.AffirmativeAndNegative);
        if (dialog == MessageDialogResult.Affirmative)
        {
            foreach (var selectedItem in selectedItems)
            {
                await ApplyTagForLineItem(selectedItem, tag);
                await _dispatcher.InvokeAsync(() => selectedItem.IsSelected = false);
            }
        }
    }

    public bool IsScanOnly { get => _isScanOnly; set => SetProperty(ref _isScanOnly, value); }

    private async Task ApplyTagForLineItem(LineItemViewModel myLineItem, string tag)
    {
        if (myLineItem.Status == tag)
        {
            return;
        }

        if (myLineItem.Id == 0)
        {
            return;
        }

        await _apiClient.UpdateLineItemStatusAsync(myLineItem.Id, tag);
    }

    private DelegateCommand _refreshCommand;

    public DelegateCommand RefreshCommand => _refreshCommand ??= new DelegateCommand(RefreshData);

    public int TotalDisplayed
    {
        get
        {
            return LineItemsView.Cast<LineItemViewModel>().Count();
        }
    }

    public int TotalItems
    {
        get
        {
            return LineItemsView.SourceCollection.Cast<LineItemViewModel>().Count();
        }
    }
    public int TotalSelected
    {
        get
        {
            return _lineItems.Where(l => l.IsSelected).Count();
        }
    }

    private void RefreshData()
    {
        Task.Run(FetchArchivedLineItemsAsync);
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

    public ArchivedViewModel(IDialogService dialogService,
        SessionVariables globalVariables, IDialogCoordinator dialogCoordinator,
        IEventAggregator eventAggregator, IMapper mapper, MyPrintService myPrintService,
        BinService binService, ApiClient apiClient, MessageBus messageBus)
    {
        _dispatcher = Application.Current.Dispatcher;

        _mapper = mapper;
        _dialogService = dialogService;
        _globalVariables = globalVariables;
        _dialogCoordinator = dialogCoordinator;
        _myPrintService = myPrintService;
        _eventAggregator = eventAggregator;
        _binService = binService;
        _apiClient = apiClient;
        _messageBus = messageBus;

        LineItemsView = CollectionViewSource.GetDefaultView(_lineItems);
        LineItemsView.SortDescriptions.Add(new SortDescription("OrderNumber", ListSortDirection.Descending));

        // _dispatcher.Invoke(() => ShippingLines.AddRange(_lineItems.Select(l => l.Shipping).Distinct().ToList()));
        PropertyChanged += OrderProcessingViewModel_PropertyChanged;

        _messageBus.ItemsUpdated += _messageBus_ItemsUpdated;
        _messageBus.ItemsAdded += _messageBus_ItemsAdded;
        _messageBus.ItemsArchived += _messageBus_ItemsArchived;

        LineItemsView.CollectionChanged += LineItemsView_CollectionChanged;

    }

    private async void _messageBus_ItemsArchived(object sender, int[] archivedItemsId)
    {
        var array = _lineItems.Where(x => archivedItemsId.Contains(x.Id)).ToArray();
        for (int i = array.Length - 1; i >= 0; i--)
        {
            var archivedItem = array[i];
            await _dispatcher.InvokeAsync(() =>
                    {
                        archivedItem.PropertyChanged -= LineItem_PropertyChanged;
                        _lineItems.Remove(archivedItem);
                    });
        }

    }

    private void LineItemsView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateMasterCheckBoxState();
        RaisePropertyChanged(nameof(TotalDisplayed));
        RaisePropertyChanged(nameof(TotalItems));
    }

    private void UpdateMasterCheckBoxState()
    {
        var allSelected = LineItemsView.Cast<LineItemViewModel>().All(x => x.IsSelected);
        var noneSelected = LineItemsView.Cast<LineItemViewModel>().All(x => !x.IsSelected);
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
        var items = await _apiClient.ListLineItemsByIdAsync(ids);
        foreach (var item in items.Select(_mapper.Map<LineItemViewModel>))
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

        var items = await _apiClient.ListLineItemsByIdAsync(ids);
        foreach (var item in items)
        {
            var lineItemVm = _lineItems.FirstOrDefault(x => x.Id == item.Id);
            if (lineItemVm != null)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    _mapper.Map(item, lineItemVm);
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

    public DelegateCommand ApplyNotesCommand
    {
        get => applyNotesCommand ??= new DelegateCommand(HandleApplyNotes, () => SelectedLineItem != null)
            .ObservesProperty(() => SelectedLineItem);
    }

    private async void HandleApplyNotes()
    {
        SelectedLineItem.Notes = Notes;
        var lineItemModel = _mapper.Map<MyLineItem>(SelectedLineItem);
        await _apiClient.UpdateLineItemAsync(lineItemModel);
    }
    private void HandleSearch()
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
            LineItemsView.Filter = o =>
            {
                var searchTextLower = SearchText.ToLower().Trim();

                if (o is LineItemViewModel o1)
                {
                    var combined = string.Join(" ", o1.Sku, o1.OrderNumber, o1.Shipping, o1.Name, o1.Customer, o1.CustomerEmail, o1.Status);
                    combined = Regex.Replace(combined, @"\s+", " ").ToLower();

                    if (searchTextLower.StartsWith("\"") && searchTextLower.EndsWith("\""))
                    {
                        return combined.Contains(searchTextLower.Trim('"'));
                    }

                    var searchTextLowerTokens = searchTextLower.Split(' ');

                    var result = searchTextLowerTokens.All(combined.Contains);

                    return result;

                    //return orderNumber.Contains(searchTextLower) || sku.Contains(searchTextLower) ||
                    //       o1.Name.ToLower().Contains(searchTextLower) || shippingLines.Contains(searchTextLower);
                }

                return true;
            };
        else
            LineItemsView.Filter = null;
    }
    private async void OrderProcessingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedLineItem):
                {
                    await UpdateUIForLineItem(SelectedLineItem);
                    break;
                }
            case nameof(SearchText):
                {
                    HandleSearch();
                    SelectedLineItem = null;
                    break;
                }

            case nameof(DetectedQr):
                {
                    if (!string.IsNullOrWhiteSpace(DetectedQr) && (DetectedQr.EndsWith("\n") || DetectedQr.EndsWith("\r") ||
                                                                   DetectedQr.EndsWith(" ")))
                    {
                        try
                        {
                            var parsedQr = MyQr.Parse(DetectedQr.Trim(" \r\n\t".ToCharArray()));

                            await _dispatcher.InvokeAsync(() => DetectedQr = "");


                            if (parsedQr != null)
                            {
                                // QrInfo = FetchQrInfo(parsedQr);
                                var lineItem = await _apiClient.GetLineItemByIdAsync(parsedQr.LineItemDatabaseId);
                                if (lineItem == null)
                                {
                                    await _dialogCoordinator.ShowMessageAsync(this, "Error", "Cannot process or show items that were already Shipped / Archived");
                                    return;
                                }

                                var lineItemVm = _mapper.Map<LineItemViewModel>(lineItem);

                                await ShowScanInfoAsync(lineItemVm);
                                if (IsScanOnly)
                                {
                                    await ActivateLineItemInView(lineItemVm);

                                    var basePath = Path.Combine(Properties.Settings.Default.PrintFilesFolder, lineItemVm.Sku);
                                    var gcrPath = Path.ChangeExtension(basePath, ".gcr");
                                    var pngPath = Path.ChangeExtension(basePath, ".png");

                                    if (File.Exists(gcrPath))
                                    {
                                        Process.Start(gcrPath);
                                    }
                                    else
                                    {
                                        if (File.Exists(pngPath))
                                        {
                                            Process.Start(pngPath);
                                        }
                                    }
                                }
                                else
                                {
                                    await ProcessItemForPrintingAsync(lineItemVm);
                                }

                            }
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception);
                            await _dialogCoordinator.ShowExceptionErrorAsync(this, exception);
                        }
                    }

                    break;
                }
        }
    }

    public bool IsFilterEnabled { get => _isFilterEnabled; set => SetProperty(ref _isFilterEnabled, value); }

    private async Task UpdateUIForLineItem(LineItemViewModel selectedLineItem)
    {
        try
        {
            if (SelectedLineItem == null)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    CurrentImage = null;
                    Notes = "";
                    Logs.Clear();
                });

                return;
            }


            await _dispatcher.InvokeAsync(() => IsFilterEnabled = false);

            var selectedLineItemId = selectedLineItem.Id;
            var selectedLineItemNotes = selectedLineItem.Notes;
            var selectedLineItemOrderId = selectedLineItem.OrderId;
            var selectedLineItemVariantId = selectedLineItem.VariantId;

            await _dispatcher.InvokeAsync(() =>
            {
                Notes = selectedLineItemNotes;
                CurrentImage = null;
                Logs.Clear();
            });

            var logs = await _apiClient.ListLogsAsync(selectedLineItemId);

            if (logs != null && logs.Any())
            {
                await _dispatcher.InvokeAsync(() => Logs.AddRange(logs));
            }

            var imagePath = await FetchProductImageAsync(selectedLineItem);
            CurrentImage = imagePath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        finally
        {
            await _dispatcher.InvokeAsync(() => IsFilterEnabled = true);
        }
    }

    private async Task ShowScanInfoAsync(LineItemViewModel lineItem)
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

    private async Task ProcessItemForPrintingAsync(LineItemViewModel lineItem)
    {
        try
        {
            _myPrintService.PrintItem(lineItem);
            var processingItemResult = await _apiClient.ProcessItemAsync(lineItem.Id);

            await ActivateLineItemInView(_mapper.Map<LineItemViewModel>(processingItemResult.LineItem));
            var prams = new Dictionary<string, string> { { "Order", $"{lineItem.Order}" } };
            var relatedLineItems = await _apiClient.ListItemsAsync(prams);

            if (processingItemResult.AllItemsPrinted)
            {
                if (lineItem.Shipping == "Sydney Warehouse / Studio")
                {
                    _dialogService.ShowAfterScanDialog("For Pickup", "Pick up order is ready.", lineItem.Id, async result =>
                    {
                        foreach (var lineItem in relatedLineItems.Select(_mapper.Map<LineItemViewModel>))
                        {
                            await ApplyTagForLineItem(lineItem, "LabelPrinted");
                        }
                    });
                }
                else
                    await _dispatcher.InvokeAsync(() =>
                    {
                        _dialogService.ShowLabelPrintingDialog(lineItem.OrderNumber, 0, "Ready to Print Shipping label?", async result =>
                        {
                            if (result.Result == ButtonResult.Yes)
                            {
                                foreach (var lineItem in relatedLineItems.Select(_mapper.Map<LineItemViewModel>))
                                {
                                    await ApplyTagForLineItem(lineItem, "LabelPrinted");
                                }

                                if (processingItemResult.LineItem.BinNumber.HasValue)
                                    await _binService.EmptyBinAsync(processingItemResult.LineItem.BinNumber.Value);

                                //Task.Run(async () =>
                                //{
                                //    var shipStationResult = await _browserService.NavigateToOrderAsync(lineItem.OrderNumber);
                                //    if (!shipStationResult)
                                //    {
                                //        WindowHelper.FocusSelf();
                                //        await _dialogCoordinator.ShowMessageAsync(this, "Order Not Found!", $"Cannot find Order #{lineItem.OrderNumber} in ShipStation!\nYou may need to refresh/reload your store in Shipstaion.");

                                //    }
                                //});

                                //WindowHelper.FocusChrome();
                            }
                        });
                    });
            }
            else
            {
                var prams2 = new Dictionary<string, string> { { "Id", $"{lineItem.Id}" } };
                var orderInfo = _apiClient.GetOrderOrderByAsync(prams2);

                if (relatedLineItems.Where(l => l.BinNumber > 0 && l.OrderId == lineItem.OrderId).Sum(x => x.PrintedQuantity) > 1)
                {
                    await _dispatcher.InvokeAsync(new Action(() =>
                    {
                        var dlgParams = new DialogParameters
                                        {
                                            { "Id", lineItem.Id },
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
                                            { "Id", lineItem.Id },
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

    private async Task ActivateLineItemInView(LineItemViewModel orderItemResult)
    {
        await _dispatcher.InvokeAsync(() =>
            SearchText = $"{orderItemResult.OrderNumber}");

        var existingLineItem = _lineItems.FirstOrDefault(o =>
               o.OrderId == orderItemResult.OrderId && o.LineItemId == orderItemResult.LineItemId);

        if (existingLineItem != null)
        {
            await _dispatcher.InvokeAsync(() =>
            {
                existingLineItem.BinNumber = orderItemResult.BinNumber;
                existingLineItem.PrintedQuantity = orderItemResult.PrintedQuantity;
                existingLineItem.Status = orderItemResult.Status;
                SelectedLineItem = existingLineItem;
            });



        }

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
            if (item is LineItemViewModel orderItem)
                orderItem.IsSelected = false;
    }


    private void CheckHighlighted(IEnumerable<object> highlighted)
    {
        foreach (var item in highlighted)
            if (item is LineItemViewModel orderItem)
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
        get { return checkBoxCommand ??= new DelegateCommand<bool?>(HandleCheckBoxCommand); }
    }

    public bool? MasterCheckBoxState { get => _masterCheckBoxState; set => SetProperty(ref _masterCheckBoxState, value); }

    private void HandleCheckBoxCommand(bool? checkBoxState)
    {
        if (checkBoxState.HasValue)
            foreach (var lineItem in LineItemsView.Cast<LineItemViewModel>())
            {
                lineItem.IsSelected = checkBoxState.Value;
            }

    }

    private async Task<string> FetchProductImageAsync(LineItemViewModel lineItem)
    {
        var lineItemOrderId = lineItem.OrderId;
        var lineItemVariantId = lineItem.VariantId;

        try
        {
            var imagePath = Path.Combine(_globalVariables.ImagesPath, $"{lineItemOrderId}-{lineItemVariantId}");
            if (File.Exists(imagePath))
            {
                return imagePath;
            }

            if (string.IsNullOrWhiteSpace(lineItem.ProductImage))
                return null;

            return await DownloadImageAsync(lineItem.ProductImage, imagePath);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    private async Task<string> DownloadImageAsync(string imageSource, string imagePath)
    {
        using var client = new WebClient();
        await client.DownloadFileTaskAsync(new Uri(imageSource), imagePath);
        return imagePath;
    }

    public DelegateCommand SearchCommand => searchCommand ??= new DelegateCommand(OnSearch, () => !string.IsNullOrWhiteSpace(SearchText) && SearchText.Length >= 3).ObservesProperty(() => SearchText);

    private async void OnSearch()
    {
        try
        {
            var searchResult = (await _apiClient.FindArchivedItemAsync(SearchText)).Map(_mapper.Map<LineItemViewModel>);
            foreach (var item in searchResult)
            {
                if (_lineItems.Any(x => x.Id == item.Id))
                    continue;

                item.PropertyChanged += LineItem_PropertyChanged;
                await _dispatcher.InvokeAsync(() =>
                {
                    _lineItems.Add(item);
                });
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            await _dialogCoordinator.ShowMessageAsync(this, "Error", e.Message);
        }
    }

    private DelegateCommand _restoreCommand;

    public DelegateCommand RestoreCommand
    {
        get { return _restoreCommand ??= new DelegateCommand(OnRestore); }

    }

    private async void OnRestore()
    {
        try
        {
            var selectedItems = _lineItems.Where(x => x.IsSelected).ToArray();
            await _apiClient.RestoreItemsAsync(selectedItems.Select(_mapper.Map<MyLineItem>));
            for (int i = selectedItems.Length - 1; i >= 0; i--)
            {
                var item = selectedItems[i];

                await _dispatcher.InvokeAsync(() =>
                {
                    item.PropertyChanged -= LineItem_PropertyChanged;
                    _lineItems.Remove(item);
                });
            }
        }
        catch (Exception e)
        {
            Logger.Error(e);
            await _dialogCoordinator.ShowMessageAsync(this, "Error", e.Message);
        }
    }

    private async void FetchArchivedLineItemsAsync()
    {
        var waitDialog = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Fetching archived orders...");
        try
        {
            waitDialog.SetIndeterminate();
            // Update UI
            await ClearUILineItems();
            var lineItems = await _apiClient.ListArchivedItemsAsync();

            if (lineItems == null)
            {
                return;
            }

            foreach (var lineItem in lineItems.Select(_mapper.Map<LineItemViewModel>))
            {
                lineItem.PropertyChanged += LineItem_PropertyChanged;
                await _dispatcher.InvokeAsync(() =>
                {
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

    private async Task ClearUILineItems()
    {
        for (int i = _lineItems.Count - 1; i >= 0; i--)
        {
            var item = _lineItems[i];
            item.PropertyChanged -= LineItem_PropertyChanged;
        }

        await _dispatcher.InvokeAsync(_lineItems.Clear);
    }


    private void LineItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        LineItemViewModel myLineItem;
        if (e.PropertyName == nameof(myLineItem.IsSelected))
        {
            RaisePropertyChanged(nameof(TotalSelected));
            UpdateMasterCheckBoxState();
        }
    }

    private string _notes;
    private DelegateCommand applyNotesCommand;
    private bool _isScanOnly;
    private DelegateCommand<bool?> checkBoxCommand;
    private bool? _masterCheckBoxState;
    private string LastTagFilter;
    private bool _isFilterEnabled = true;
    private DelegateCommand searchCommand;

    public DelegateCommand OpenQrScannerCommand
    {
        get { return _openQrScannerCommand ??= new DelegateCommand(OpenScanner); }
    }

    private void OpenScanner()
    {
        _dialogService.ShowDialog("ScannerView");
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

    public LineItemViewModel SelectedLineItem
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
        Task.Run(FetchArchivedLineItemsAsync);
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