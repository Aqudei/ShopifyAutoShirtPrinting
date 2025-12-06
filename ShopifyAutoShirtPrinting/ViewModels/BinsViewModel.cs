using AutoMapper;
using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class BinsViewModel : PageBase, INavigationAware
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly BinService _binService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;
        private readonly ApiClient _apiClient;
        private readonly IMapper _mapper;
        private DelegateCommand<BinViewModel> clearBinCommand;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override string Title => "Bins";
        private ObservableCollection<BinViewModel> _bins = new();

        public ICollectionView Bins { get => bins; set => SetProperty(ref bins, value); }

        public DelegateCommand<BinViewModel> ClearBinCommand { get => clearBinCommand ??= new DelegateCommand<BinViewModel>(HandleClearBin); }
        private string _searchText;
        private ICollectionView bins;

        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }

        private async void HandleClearBin(BinViewModel bin)
        {
            var result = await _dialogCoordinator.ShowMessageAsync(this, "Confirm", $"Do you really want to empty Bin with No. #{bin.BinNumber}?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                await _binService.EmptyBinAsync(bin.BinNumber);
            }
        }

        private DelegateCommand<BinViewModel> _openInBrowserCommand;

        public DelegateCommand<BinViewModel> OpenInBrowserCommand
        {
            get { return _openInBrowserCommand ??= new DelegateCommand<BinViewModel>(HandleOpenInBrowwer); }
        }

        private void HandleOpenInBrowwer(BinViewModel bin)
        {
            //Task.Run(async () =>
            //{
            //    var shipStationResult = await _browserService.NavigateToOrderAsync(bin.OrderNumber);
            //    if (!shipStationResult)
            //    {
            //        WindowHelper.FocusSelf();
            //        await _dialogCoordinator.ShowMessageAsync(this, "Order Not Found!", $"Cannot find Order #{bin.OrderNumber} in ShipStation!\nYou may need to refresh/reload your store in Shipstaion.");
            //    }
            //});
            //WindowHelper.FocusChrome();
        }

        private DelegateCommand<BinViewModel> _editNotesCommand;

        public DelegateCommand<BinViewModel> EditNotesCommand
        {
            get { return _editNotesCommand ??= new DelegateCommand<BinViewModel>(HandleEditNotes); }
        }

        private async void HandleEditNotes(BinViewModel bin)
        {
            var binModel = _mapper.Map<Bin>(bin);

            if (bin.HasNotes)
            {
                var dialogParams = new DialogParameters {
                    {"title","Edit Notes" },
                    {"text",bin.Notes }
                };

                _dialogService.ShowDialog("EditTextDialog", dialogParams, async r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue<string>("text", out var newNote))
                        {
                            binModel.Notes = newNote;
                            await _apiClient.UpdateBinAsync(binModel);
                        }
                    }
                });
            }
            else
            {
                var result = await _dialogCoordinator.ShowInputAsync(this, "Edit Notes", "");
                binModel.Notes = result;
                await _apiClient.UpdateBinAsync(binModel);
            }
        }
        private DelegateCommand<BinViewModel> _deleteNotesCommand;

        public DelegateCommand<BinViewModel> DeleteNotesCommand
        {
            get { return _deleteNotesCommand ??= new DelegateCommand<BinViewModel>(HandleDeleteNotes); }
        }

        private async void HandleDeleteNotes(BinViewModel bin)
        {
            var binModel = _mapper.Map<Bin>(bin);
            binModel.Notes = "";
            await _apiClient.UpdateBinAsync(binModel);
        }

        public BinsViewModel(IDialogCoordinator dialogCoordinator, BinService binService, IDialogService dialogService,
            IMessageBus messageBus, ApiClient apiClient, IMapper mapper)
        {
            _mapper = mapper;
            _apiClient = apiClient;
            _messageBus = messageBus;
            _binService = binService;
            _dialogService = dialogService;
            _dialogCoordinator = dialogCoordinator;
            
            Bins = CollectionViewSource.GetDefaultView(_bins);
            Bins.GroupDescriptions.Add(new PropertyGroupDescription("StoreName"));

            _messageBus.BinsDestroyed += _messageBus_BinsDestroyed;
            _messageBus.BinsUpdated += _messageBus_BinsUpdated;

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SearchText):
                        {
                            if (SearchText.Count() >= 3)
                            {
                                Bins.Filter = (e) =>
                                {
                                    var bin = e as BinViewModel;
                                    var nameMatched = bin.LineItems.Any(x => x.Name.ToLower().Contains(SearchText.ToLower()));
                                    return bin.BinNumber.ToString().Contains(SearchText) || bin.OrderNumber.ToString().Contains(SearchText) || nameMatched;
                                };
                            }
                            else
                            {
                                Bins.Filter = null;
                            }

                            break;
                        }
                    default:
                        break;
                }
            };
        }

        private async void _messageBus_BinsUpdated(object sender, int[] binNumbers)
        {
            try
            {
                foreach (var binNumber in binNumbers)
                {
                    var q = new System.Collections.Generic.Dictionary<string, string> { { "Number", $"{binNumber}" } };
                    var result = await _apiClient.FindBinsAsync(q);
                    if (result != null && result.Length > 0)
                    {
                        var uiBin = _bins.FirstOrDefault(x => x.BinNumber == result[0].BinNumber);
                        if (uiBin != null)
                        {
                            await _dispatcher.InvokeAsync(() => _mapper.Map(result[0], uiBin));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void _messageBus_BinsDestroyed(object sender, int[] binNumbers)
        {
            try
            {
                var bins = _bins.ToArray();

                for (int i = bins.Length - 1; i >= 0; i--)
                {
                    var bin = bins[i];
                    if (binNumbers.Contains(bin.BinNumber))
                    {
                        _dispatcher.Invoke(() => _bins.Remove(bin));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task LoadBins()
        {
            await _dispatcher.InvokeAsync(_bins.Clear);

            var bins = await _binService.ListBinsAsync();
            await _dispatcher.InvokeAsync(() =>
            {;
                _bins.AddRange(bins);
            });

        }



        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Task.Run(async () =>
            {
                var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please", "Fetching Bins info...");
                progress.SetIndeterminate();

                try
                {
                    await LoadBins();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message);
                }
                finally
                {
                    await progress.CloseAsync();
                }
            });
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
