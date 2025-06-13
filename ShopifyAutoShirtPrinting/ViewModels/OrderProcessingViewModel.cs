using AutoMapper;
using Common.Api;
using Common.Models;
using ImTools;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using ControlzEx.Standard;
using ZXing;
using ZXing.Common;
using Application = System.Windows.Application;
using Path = System.IO.Path;
using PrintDocument = System.Drawing.Printing.PrintDocument;

namespace ShopifyEasyShirtPrinting.ViewModels;

public class OrderProcessingViewModel : PageBase, INavigationAware
{

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<LineItemViewModel> _lineItems = [];
    public ICollectionView LineItemsView { get; }
    public override string Title => "Order Items";
    private string _currentImage;
    private DelegateCommand _generateQrCommand;
    private LineItemViewModel _selectedLineItem;
    private readonly IDialogService _dialogService;
    private readonly SessionVariables _globalVariables;
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly IMapper _mapper;
    private readonly BinService _binService;
    private readonly ApiClient _apiClient;
    private readonly IMessageBus _messageBus;
    private readonly IEventAggregator _eventAggregator;
    private DelegateCommand _openQrScannerCommand;
    private string _searchText;
    private string _detectedQr;

    public Dictionary<string, bool> ColumnVisibility
    {
        get => _columnVisibility;
        set => SetProperty(ref _columnVisibility, value);
    }

    public string[] ScanModes =>
    [
        SCAN_MODE_PROCESSING,
        SCAN_MODE_SCAN_ONLY,
        SCAN_MODE_GCR_FRONT,
        SCAN_MODE_GCR_BACK
    ];

    private string _selectedScanMode;

    public string SelectedScanMode
    {
        get => _selectedScanMode;
        set => SetProperty(ref _selectedScanMode, value);
    }

    private const string SCAN_MODE_SCAN_ONLY = "Scan Only";
    private const string SCAN_MODE_PROCESSING = "Processing";
    private const string SCAN_MODE_GCR_FRONT = "Open GCR (Front)";
    private const string SCAN_MODE_GCR_BACK = "Open GCR (Back)";

    public ObservableCollection<string> ShippingLines { get; set; } = new();

    private DelegateCommand _saveQrTagsCommand;
    private DelegateCommand<string> _applyTagCommand;
    public ObservableCollection<Log> Logs { get; set; } = new();
    public ObservableCollection<KeyValuePair<string, string>> ScanInfo { set; get; } = new();
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

    public string[] Tags =>
    [
        "Pending",
        "Processed",
        "LabelPrinted",
        "Shipped",
        "Archived",
        "Issue Needs Resolving",
        "Need To Order From Supplier",
        "Have Ordered From Supplier"
    ];

    public string[] TagsFilter =>
    [
        "Pending",
        "Processed",
        "LabelPrinted",
        "Shipped",
        "Issue Needs Resolving",
        "Need To Order From Supplier",
        "Have Ordered From Supplier"
    ];

    private string _selectedTagFilter;
    public string SelectedTagFilter
    {
        get { return _selectedTagFilter; }
        set { SetProperty(ref _selectedTagFilter, value); }
    }

    private DelegateCommand<string> _printCommand;

    public DelegateCommand<string> PrintCommand => _printCommand ??= new DelegateCommand<string>(HandlePrintCommand, arg => SelectedLineItem != null)
        .ObservesProperty(() => SelectedLineItem);

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

    public DelegateCommand<string> ApplyTagCommand
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
        // Removed
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

    public DelegateCommand SaveQrTagsCommand => _saveQrTagsCommand ??= new DelegateCommand(OnSaveQrTag, () => TotalSelected > 0)
        .ObservesProperty(() => TotalSelected);


    private DelegateCommand _refreshCommand;

    public DelegateCommand RefreshCommand => _refreshCommand ??= new DelegateCommand(RefreshData);


    private DelegateCommand<string> _crudCommand;

    public DelegateCommand<string> CrudCommand
    {
        get { return _crudCommand ??= new DelegateCommand<string>(OnCrudCommand); }
    }

    private void OnCrudCommand(string crudCommand)
    {
        switch (crudCommand.ToUpper())
        {
            case "CREATE":
                _dialogService.ShowDialog("CrudDialog", callback: async (r) =>
                {
                    if (r.Parameters.TryGetValue<MyLineItem>("MyLineItem", out var myLineItem))
                    {
                        var createdLineItem = await _apiClient.CreateLineItemForStoreAsync(_globalVariables.ActiveStore, myLineItem);
                    }
                });
                break;
            case "UPDATE":
                if (SelectedLineItem == null)
                    return;

                var prams = new DialogParameters { { "MyLineItem", _mapper.Map<MyLineItem>(SelectedLineItem) } };
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
        get => _totalDisplayed; set => SetProperty(ref _totalDisplayed, value);
    }

    public int TotalItems
    {
        get => _totalItems; set => SetProperty(ref _totalItems, value);
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
        await Task.Run(FetchActiveLineItemsAsync);
        await ClearUIItemInfo();
    }

    private async void OnSaveQrTag()
    {
        var dlg = new CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        var result = dlg.ShowDialog();
        if (result == CommonFileDialogResult.Ok)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Saving Qr Tags");
            IEnumerable<LineItemViewModel> selectedItems = _lineItems.Where(l => l.IsSelected).ToArray();
            foreach (var orderItem in selectedItems)
            {

                var hasBackPrintValue = false;
                if (orderItem.VariantId.HasValue && orderItem.VariantId.Value > 0)
                {
                    var variant = await _apiClient.FindVariant(orderItem.VariantId);
                    if (variant != null && variant.Product != null)
                    {
                        hasBackPrintValue = variant.HasBackPrint;
                    }
                }


                using var combinedImage = GenerateQrImageForItem(orderItem, hasBackPrintValue);

                var outputName = $"{orderItem.OrderNumber}-{orderItem.Id}-{orderItem.Name}.png";
                outputName = DirectoryHelper.SanitizeFilename(RemoveRedundantChars(outputName.Replace("/", "-").Replace("\\", "-").Replace(" ", "-"), "-"));
                var finalQrPath = Path.Combine(dlg.FileName, outputName);
                combinedImage.Save(finalQrPath);
            }
            await progress.CloseAsync();
            Process.Start("explorer.exe", $"\"{dlg.FileName}\"");
        }
    }

    private Bitmap GenerateQrImageForItem(LineItemViewModel lineItem, bool hasBackPrint = false)
    {
        var qrData = new[] {
                    $"{lineItem.Id}",
                    $"{lineItem.OrderNumber}"
                };

        bool? color = null;
        bool? isDtf = null;

        if (!string.IsNullOrWhiteSpace(lineItem.Sku))
        {
            isDtf = lineItem.Sku.ToUpper().EndsWith("-DTF") ? true : false;

            if (isDtf.Value)
            {
                color = null;
            }
            else
            {
                color = lineItem.Sku.ToUpper().EndsWith("-LT") ? true : false;
            }
        }

        var hasNotes = !string.IsNullOrWhiteSpace(lineItem.Notes);

        var qrDataText = EncodeText(string.Join($"{Environment.NewLine}", qrData));

        var qrHelper = new QrHelper();

        using var qrImage = qrHelper.GenerateBitmapQr(qrDataText);
        using var textImage = qrHelper.DrawQrTagInfo(lineItem.Name, qrImage, lineItem.OrderNumber, hasNotes, color, hasBackPrint, isDtf);
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

    public OrderProcessingViewModel(IDialogService dialogService,
        SessionVariables globalVariables, IDialogCoordinator dialogCoordinator,
        IEventAggregator eventAggregator, IMapper mapper, MyPrintService myPrintService,
        BinService binService, ApiClient apiClient, IMessageBus messageBus)
    {
        _dispatcher = Application.Current.Dispatcher;

        _mapper = mapper;
        _dialogService = dialogService;
        _globalVariables = globalVariables;
        _dialogCoordinator = dialogCoordinator;
        _eventAggregator = eventAggregator;
        _binService = binService;
        _apiClient = apiClient;
        _messageBus = messageBus;

        SelectedScanMode = ScanModes[0];

        LineItemsView = CollectionViewSource.GetDefaultView(_lineItems);
        LineItemsView.SortDescriptions.Add(new SortDescription("OrderNumber", ListSortDirection.Descending));

        // _dispatcher.Invoke(() => ShippingLines.AddRange(_lineItems.Select(l => l.Shipping).Distinct().ToList()));
        PropertyChanged += OrderProcessingViewModel_PropertyChanged;

        _messageBus.ItemsUpdated += _messageBus_ItemsUpdated;
        _messageBus.ItemsAdded += _messageBus_ItemsAdded;
        _messageBus.ItemsArchived += _messageBus_ItemsArchived;

        //LineItemsView.CollectionChanged += LineItemsView_CollectionChanged;
    }

    private void FocusSelf()
    {
        var dt = new DispatcherTimer(DispatcherPriority.Normal);
        dt.Interval = TimeSpan.FromSeconds(2);
        dt.Tick += (s, e) =>
        {
            WindowHelper.FocusSelf();
            dt.Stop();
        };
        dt.Start();
    }



    private void UpdateCounts()
    {
        TotalDisplayed = LineItemsView.Cast<LineItemViewModel>().Count();
        TotalItems = LineItemsView.SourceCollection.Cast<LineItemViewModel>().Count();
        RaisePropertyChanged(nameof(TotalSelected));
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
    private void _messageBus_ItemsArchived(object sender, int[] archivedItemsId)
    {
        Task.Run(() => HandleOnItemsArchivedTaskAsync(archivedItemsId));
    }

    private async Task HandleOnItemsArchivedTaskAsync(int[] archivedItemsId)
    {
        try
        {
            var archivedIdsSet = new HashSet<int>(archivedItemsId);
            var itemsToRemove = _lineItems.Where(x => archivedIdsSet.Contains(x.Id)).ToList();

            if (itemsToRemove.Count == 0)
                return;

            await _dispatcher.InvokeAsync(() =>
            {
                foreach (var item in itemsToRemove)
                {
                    item.PropertyChanged -= LineItem_PropertyChanged;
                    _lineItems.Remove(item);
                }
            });
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    private void _messageBus_ItemsAdded(object sender, int[] ids)
    {
        Task.Run(() => HandleItemsAddedTaskAsync(ids));
    }

    private async Task HandleItemsAddedTaskAsync(int[] ids)
    {
        Debug.WriteLine($"@_messageBus_ItemsAdded() -> {ids}");

        try
        {
            var existingIds = _lineItems.Select(x => x.Id).ToHashSet();
            var items = await _apiClient.ListLineItemsByIdAsync(ids);

            var toAdd = items
                .Select(_mapper.Map<LineItemViewModel>)
                .Where(item => !existingIds.Contains(item.Id))
                .ToList();

            foreach (var item in toAdd)
            {
                item.PropertyChanged += LineItem_PropertyChanged;
            }

            if (toAdd.Count > 0)
            {
                await _dispatcher.InvokeAsync(() => _lineItems.AddRange(toAdd));
            }

            await UpdateDisplayAsync();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }


    private async Task UpdateDisplayAsync()
    {
        await _dispatcher.InvokeAsync(() =>
        {
            UpdateCounts();
            UpdateMasterCheckBoxState();
        });
    }

    private void _messageBus_ItemsUpdated(object sender, int[] ids)
    {
        Task.Run(() => HandleItemsUpdatedTaskAsync(ids));
    }

    private async Task HandleItemsUpdatedTaskAsync(int[] ids)
    {
        Debug.WriteLine($"@_messageBus_ItemsUpdated() -> {ids}");

        try
        {
            var itemMap = _lineItems.ToDictionary(x => x.Id);
            var items = await _apiClient.ListLineItemsByIdAsync(ids);

            var updates = new List<Action>();

            foreach (var item in items)
            {
                if (itemMap.TryGetValue(item.Id, out var lineItemVm))
                {
                    updates.Add(() => _mapper.Map(item, lineItemVm));
                }
            }

            if (updates.Count > 0)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    foreach (var update in updates)
                    {
                        update();
                    }
                });
            }

            await UpdateDisplayAsync();
        }
        catch (Exception e)
        {
            Logger.Error(e);
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

    private async void HandleTagFilter(string statusTag)
    {
        try
        {
            if (statusTag == "Archived")
            {
                await FetchArchivedLineItemsAsync();
            }
            else
            {
                if (LastTagFilter == "Archived")
                    await FetchActiveLineItemsAsync();
            }

            LineItemsView.Filter = s =>
            {
                if (s is LineItemViewModel o1)
                {
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        return SearchFunction(o1) && o1.Status == statusTag;

                        //return orderNumber.Contains(searchTextLower) || sku.Contains(searchTextLower) ||
                        //       o1.Name.ToLower().Contains(searchTextLower) || shippingLines.Contains(searchTextLower);

                    }
                    return o1.Status == statusTag;
                }

                return true;
            };

            await UpdateDisplayAsync();
        }
        catch (Exception e)
        {
            Logger.Error(e);
            await _dialogCoordinator.ShowMessageAsync(this, "Something went wrong", e.Message);
        }
    }

    private bool SearchFunction(LineItemViewModel o1)
    {
        var searchTextLower = SearchText.ToLower().Trim();

        var combined = string.Join(" ", o1.Sku, o1.OrderNumber, o1.Shipping, o1.Name, o1.Customer, o1.CustomerEmail, o1.Notes);
        combined = Regex.Replace(combined, @"\s+", " ").ToLower();

        if (searchTextLower.StartsWith("\"") && searchTextLower.EndsWith("\""))
        {
            return combined.Contains(searchTextLower.Trim('"'));
        }

        var searchTextLowerTokens = searchTextLower.Split(' ');

        var result = searchTextLowerTokens.All(combined.Contains);

        return result;
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

    private DelegateCommand _clearStatusFilterCommand;

    public DelegateCommand ClearStatusFilterCommand
    {
        get { return _clearStatusFilterCommand ??= new DelegateCommand(HandleClearStatusFilter); }
    }

    private async void HandleClearStatusFilter()
    {
        try
        {
            if (LastTagFilter == "Archived")
            {
                await Task.Run(FetchActiveLineItemsAsync);
            }

            await _dispatcher.InvokeAsync(() =>
            {
                SelectedTagFilter = null;
            });


            await Task.Run(HandleSearchAsync);

        }
        catch (Exception e)
        {
            Logger.Error(e);
            await _dialogCoordinator.ShowMessageAsync(this, "Error", e.Message);
        }
    }

    private async void OrderProcessingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        try
        {
            switch (e.PropertyName)
            {
                case nameof(SelectedTagFilter):
                    {
                        SelectedLineItem = null;
                        HandleTagFilter(SelectedTagFilter);
                        LastTagFilter = SelectedTagFilter;
                        break;
                    }
                case nameof(SelectedLineItem):
                    {
                        await UpdateUiForLineItem(SelectedLineItem);
                        break;
                    }
                case nameof(SearchText):
                    {
                        SelectedLineItem = null;
                        await HandleSearchAsync();
                        break;
                    }

                case nameof(DetectedQr):
                    {
                        if (string.IsNullOrWhiteSpace(DetectedQr) ||
                            !DetectedQr.EndsWith("\n") && !DetectedQr.EndsWith("\r") && !DetectedQr.EndsWith(" "))
                        {
                            break;
                        }

                        try
                        {
                            var parsedQr = MyQr.Parse(DetectedQr.TrimEnd(' ', '\r', '\n'));

                            await _dispatcher.InvokeAsync(() =>
                            {
                                DetectedQr = "";
                                HandleClearStatusFilter();
                                SearchText = "";
                            });

                            if (parsedQr == null)
                            {
                                break;
                            }

                            var lineItem = await _apiClient.GetLineItemByIdAsync(parsedQr.LineItemDatabaseId);
                            if (lineItem == null)
                            {
                                await _dialogCoordinator.ShowMessageAsync(this, "Error", "Cannot process or show items that were already Shipped / Archived");
                                break;
                            }

                            var lineItemVm = _mapper.Map<LineItemViewModel>(lineItem);
                            await ShowScanInfoAsync(lineItemVm);
                            await ActivateLineItemInView(lineItemVm);

                            switch (SelectedScanMode)
                            {
                                case SCAN_MODE_GCR_FRONT:
                                    await TryOpenPrintFiles(lineItemVm);
                                    break;
                                case SCAN_MODE_GCR_BACK:
                                    await TryOpenPrintFiles(lineItemVm, true);
                                    break;
                                case SCAN_MODE_SCAN_ONLY:
                                    break;
                                default:
                                    await ProcessItemForPrintingAsync(lineItemVm);
                                    break;
                            }
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception);
                            await _dialogCoordinator.ShowExceptionErrorAsync(this, exception);
                        }

                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            await _dialogCoordinator.ShowExceptionErrorAsync(this, ex);

        }
    }

    private void SetLineItemsViewFilter(Predicate<object> filter)
    {
        _dispatcher.BeginInvoke(() =>
        {
            LineItemsView.Filter = filter;
        });
    }

    private async Task HandleSearchAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
                SetLineItemsViewFilter(o =>
                {
                    if (o is LineItemViewModel o1)
                    {
                        if (string.IsNullOrWhiteSpace(SelectedTagFilter))
                            return SearchFunction(o1);
                        else
                        {
                            return SearchFunction(o1) && o1.Status == SelectedTagFilter;
                        }


                        //return orderNumber.Contains(searchTextLower) || sku.Contains(searchTextLower) ||
                        //       o1.Name.ToLower().Contains(searchTextLower) || shippingLines.Contains(searchTextLower);
                    }

                    return true;
                });
            else
            {
                if (string.IsNullOrWhiteSpace(SelectedTagFilter))
                    SetLineItemsViewFilter(null);
                else
                {
                    SetLineItemsViewFilter(o =>
                    {
                        if (o is LineItemViewModel o1)
                        {
                            return o1.Status == SelectedTagFilter;
                        }

                        return true;
                    });
                }
            }

            await UpdateDisplayAsync();
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    private async Task TryOpenPrintFiles(LineItemViewModel lineItemVm, bool backPrint = false)
    {
        if (lineItemVm == null) return;

        string printFolder = Properties.Settings.Default.PrintFilesFolder;
        string sku = lineItemVm.Sku;

        // Try direct print files path first if SKU and PrintFilesFolder are present
        if (!string.IsNullOrWhiteSpace(sku) && !string.IsNullOrWhiteSpace(printFolder))
        {
            string basePath = backPrint
                ? Path.Combine(printFolder, "backprints", sku)
                : Path.Combine(printFolder, sku);

            string gcrPath = Path.ChangeExtension(basePath, ".gcr");
            string pngPath = Path.ChangeExtension(basePath, ".png");

            if (File.Exists(gcrPath))
            {
                Process.Start(gcrPath);
                return;
            }

            if (File.Exists(pngPath))
            {
                Process.Start(pngPath);
                return;
            }
        }

        // If design-based fallback is needed
        if (lineItemVm.Designs == null || !lineItemVm.Designs.Any() || string.IsNullOrWhiteSpace(Properties.Settings.Default.GarmentCreatorPath))
            return;

        
        if(File.Exists(Properties.Settings.Default.GarmentCreatorPath) == false)
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", "Garment Creator path is not set or the file does not exist.");
            return;
        }

        string keyword = backPrint ? "back" : "front";
        var design = lineItemVm.Designs
            .FirstOrDefault(d => d.Name?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            ?? lineItemVm.Designs.First();

        string fileName = Path.GetFileName(design.ImageValue);
        string localFilePath = Path.Combine(_globalVariables.ImagesPath, fileName);

        if (File.Exists(localFilePath))
        {
            Process.Start(Properties.Settings.Default.GarmentCreatorPath, $"\"{localFilePath}\"" );
        }
        else
        {
            await PrintHelpers.DownloadRemoteFileToLocalAsync(design.ImageValue, localFilePath);
            Process.Start(Properties.Settings.Default.GarmentCreatorPath, $"\"{localFilePath}\"");
        }
    }


    public bool IsFilterEnabled { get => _isFilterEnabled; set => SetProperty(ref _isFilterEnabled, value); }

    private async Task UpdateUiForLineItem(LineItemViewModel theSelectedLineItem)
    {
        try
        {
            if (theSelectedLineItem == null)
            {
                await ClearUIItemInfo();

                return;
            }

            await _dispatcher.InvokeAsync(() => IsFilterEnabled = false);

            var selectedLineItemId = theSelectedLineItem.Id;
            var selectedLineItemNotes = theSelectedLineItem.Notes;
            var selectedLineItemOrderId = theSelectedLineItem.OrderId;
            var selectedLineItemVariantId = theSelectedLineItem.VariantId;


            await _dispatcher.InvokeAsync(() =>
                        {
                            Notes = selectedLineItemNotes;
                            CurrentImage = null;
                        });

            var logs = await _apiClient.ListLogsAsync(selectedLineItemId);

            if (logs != null && logs.Any())
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    Logs.Clear();
                    Logs.AddRange(logs);
                });
            }

            var imagePath = await FetchProductImageAsync(theSelectedLineItem);
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

    private async Task ClearUIItemInfo()
    {
        await _dispatcher.InvokeAsync(() =>
        {
            Notes = string.Empty;
            CurrentImage = null;
            Logs.Clear();
        });
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

    private async Task ProcessItemForPrintingAsync(LineItemViewModel lineItemVm)
    {
        try
        {
            // await _myPrintService.PrintItem(lineItemVm);

            var processingItemResult = await _apiClient.ProcessItemAsync(lineItemVm.Id);

            await ActivateLineItemInView(_mapper.Map<LineItemViewModel>(processingItemResult.LineItem));
            var dialogParameters = new Dictionary<string, string> { { "OrderNumber", $"{lineItemVm.OrderNumber}" } };
            var relatedLineItems = await _apiClient.ListItemsAsync(dialogParameters);

            if (processingItemResult.AllItemsPrinted)
            {
                if (lineItemVm.ForPickup)
                {
                    _dialogService.ShowAfterScanDialog("For Pickup", "Pick up order is ready.", lineItemVm.Id, async result =>
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
                        _dialogService.ShowLabelPrintingDialog(lineItemVm.OrderNumber, _globalVariables.ActiveStore.Id, "Ready to Print Shipping label?", async result =>
                        {
                            if (result.Result == ButtonResult.Yes)
                            {
                                if (result.Parameters.TryGetValue<bool>("auspost", out var isAusPost) && isAusPost)
                                {

                                }
                                else
                                {
                                    foreach (var lineItem in relatedLineItems.Select(_mapper.Map<LineItemViewModel>))
                                    {
                                        await ApplyTagForLineItem(lineItem, "LabelPrinted");
                                    }

                                    if (processingItemResult.LineItem.BinNumber.HasValue)
                                        await _binService.EmptyBinAsync(processingItemResult.LineItem.BinNumber.Value);
                                }
                            }
                        });
                    });
            }
            else
            {
                var prams2 = new Dictionary<string, string> { { "Id", $"{lineItemVm.Id}" } };
                var orderInfo = _apiClient.GetOrderOrderByAsync(prams2);

                if (relatedLineItems.Where(l => l.BinNumber > 0 && l.OrderId == lineItemVm.OrderId).Sum(x => x.PrintedQuantity) > 1)
                {
                    await _dispatcher.InvokeAsync(new Action(() =>
                    {
                        var dlgParams = new DialogParameters
                                        {
                                            { "Id", lineItemVm.Id },
                                            { "Message", "Added item to existing bin:" },
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
                                            { "Id", lineItemVm.Id },
                                            { "Message", "This item goes into new bin:" },
                                            { "Title", "NEW BIN CREATED" }
                                        };

                        _dialogService.ShowDialog("AfterScanDialog", dlgParams, result => { });
                    }));
                }
            }
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
        get => checkBoxCommand ??= new DelegateCommand<bool?>(HandleCheckBoxCommand);
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
        if (lineItem == null)
            return null;

        var lineItemOrderId = lineItem.OrderId;
        var lineItemVariantId = lineItem.VariantId;

        try
        {
            var imagePath = Path.Combine(_globalVariables.ImagesPath, $"{lineItem.Store}-{lineItemOrderId}-{lineItemVariantId}");
            if (File.Exists(imagePath))
            {
                return imagePath;
            }

            if (string.IsNullOrWhiteSpace(lineItem.ProductImage))
                return null;

            return await PrintHelpers.DownloadRemoteFileToLocalAsync(lineItem.ProductImage, imagePath);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    private async Task FetchArchivedLineItemsAsync()
    {
        var waitDialog = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Fetching archived orders...");
        waitDialog.SetIndeterminate();

        try
        {
            // Update UI
            await ClearUiLineItems();
            var lineItems = await _apiClient.ListArchivedItemsAsync();

            if (lineItems == null)
            {
                return;
            }

            foreach (var lineItem in lineItems.Select(_mapper.Map<LineItemViewModel>))
            {
                lineItem.PropertyChanged += LineItem_PropertyChanged;
                await _dispatcher.InvokeAsync(() => _lineItems.Add(lineItem));
            }

            UpdateCounts();
            UpdateMasterCheckBoxState();
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

    private async Task ClearUiLineItems()
    {
        foreach (var item in _lineItems)
        {
            item.PropertyChanged -= LineItem_PropertyChanged;
        }

        await _dispatcher.InvokeAsync(_lineItems.Clear);

        GC.Collect();
    }

    private async Task FetchActiveLineItemsAsync()
    {
        var waitDialog = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Fetching orders...");
        waitDialog.SetIndeterminate();

        try
        {
            await ClearUiLineItems();

            var lineItems = await _apiClient.ListItemsAsync(storeId: _globalVariables.ActiveStore.Id);
            if (lineItems?.Any() != true)
            {
                return;
            }

            var mappedItems = lineItems.Select(_mapper.Map<LineItemViewModel>).ToList();

            foreach (var lineItem in mappedItems)
            {
                lineItem.PropertyChanged += LineItem_PropertyChanged;
            }

            await _dispatcher.InvokeAsync(() => _lineItems.AddRange(mappedItems));
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


    private async void LineItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        LineItemViewModel myLineItem;
        try
        {
            if (e.PropertyName == nameof(myLineItem.IsSelected))
            {
                await UpdateDisplayAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    private string _notes;
    private DelegateCommand applyNotesCommand;
    private bool _isScanOnly;
    private DelegateCommand<bool?> checkBoxCommand;
    private bool? _masterCheckBoxState;
    private string LastTagFilter;
    private bool _isFilterEnabled = true;
    private int _totalDisplayed;
    private int _totalItems;
    private Store _store;

    public DelegateCommand OpenQrScannerCommand
    {
        get { return _openQrScannerCommand ??= new DelegateCommand(OpenScanner); }
    }

    private void OpenScanner()
    {
        _dialogService.ShowDialog("ScannerView");
    }

    public DelegateCommand GenerateQrCommand => _generateQrCommand ??= new DelegateCommand(OnPrintQrTags, () => TotalSelected > 0)
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

    private async void OnPrintQrTags()
    {
        try
        {
            await PrintQrTagsAsync();
        }
        catch (Exception e)
        {
            Logger.Error(e);
            await _dialogCoordinator.ShowMessageAsync(this, "Error", e.Message);

        }
    }

    private async Task PrintQrTagsAsync()
    {
        if (!_lineItems.Any(l => l.IsSelected))
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", "Please select items to generate QR for!");
            return;
        }

        var selectedItems = _lineItems.Where(l => l.IsSelected).ToArray();

        var progress = await _dialogCoordinator.ShowProgressAsync(this, "Print", "Please wait while printing QR Tags");
        progress.SetIndeterminate();


        try
        {
            foreach (var lineItem in selectedItems)
            {
                var hasBackPrintValue = false;
                if (lineItem.VariantId > 0)
                {
                    var variant = await _apiClient.FindVariant(lineItem.VariantId);
                    if (variant != null && variant.Product != null)
                    {
                        hasBackPrintValue = variant.HasBackPrint;
                    }
                }

                var combinedImage = GenerateQrImageForItem(lineItem, hasBackPrintValue);

                for (int i = 0; i < lineItem.Quantity; i++)
                {
                    PrintQr(combinedImage);
                }

                await _dispatcher.InvokeAsync(() => lineItem.IsSelected = false);
                await _apiClient.UpdateLineItemStatusAsync(lineItem.Id, "Processed");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        finally
        {
            await progress.CloseAsync();
        }
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
    // public Store Store { get => _store; set => SetProperty(ref _store, value); }

    private DelegateCommand<Common.Models.Store> _moveOrderToStoreCommand;
    private Dictionary<string, bool> _columnVisibility;

    public DelegateCommand<Common.Models.Store> MoveOrderToStoreCommand
    {
        get { return _moveOrderToStoreCommand ??= new DelegateCommand<Common.Models.Store>(OnMoveOrderToStore); }
    }

    private async void OnMoveOrderToStore(Common.Models.Store selectedStore)
    {
        if (selectedStore == null || !_lineItems.Where(i => i.IsSelected).Any())
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Warning", "No Order to move.");
            return;
        }

        Logger.Info($"Moving selected order/s to store: {selectedStore.Name}");

        var selected = _lineItems.Where(i => i.IsSelected);

        var message = $"Are you sure you want to move the selected items, along with any line items " +
            "associated with the same orders, to \"{store?.Name}\" ?\n\n";
        message += string.Join("\n", selected.Take(10).Select(s => $" - {s.Name}"));
        var remaining = selected.Skip(10).Count();
        if (remaining > 0)
        {
            message += $"\n{remaining} more items...";
        }
        var prompt = await _dialogCoordinator.ShowMessageAsync(this, "Confirm action", message, MessageDialogStyle.AffirmativeAndNegative);
        if (prompt == MessageDialogResult.Affirmative)
        {
            var orderIds = _lineItems.Where(i => i.IsSelected).Select(l => l.Id).ToHashSet();
            await _apiClient.MoveOrdersToStoreAsync(selectedStore.Id, orderIds);
            await Task.Run(RefreshData);
        }
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        Debug.WriteLine(navigationContext.Parameters);

        AddRemoveDataGridColumns();

        Task.Run(FetchActiveLineItemsAsync);
    }

    private void AddRemoveDataGridColumns()
    {
        if (_globalVariables.ActiveStore.Name.Contains("Louie"))
        {
            ColumnVisibility = new Dictionary<string, bool> { { "Sku", false } };
        }
        else
        {
            ColumnVisibility = new Dictionary<string, bool> { { "Sku", true } };
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;


    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        Debug.WriteLine(navigationContext.Parameters);
    }
}

// 170.64.158.123