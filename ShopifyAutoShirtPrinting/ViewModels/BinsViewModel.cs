using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
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
        private readonly IShipStationBrowserService _browserService;
        private readonly MessageBus _messageBus;
        private DelegateCommand<Bin> clearBinCommand;

        public override string Title => "Bins";
        private ObservableCollection<Bin> _bins = new();

        public ICollectionView Bins { get => bins; set => SetProperty(ref bins, value); }

        public DelegateCommand<Bin> ClearBinCommand { get => clearBinCommand ??= new DelegateCommand<Bin>(HandleClearBin); }
        private string _searchText;
        private ICollectionView bins;

        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }

        private async void HandleClearBin(Bin bin)
        {
            var result = await _dialogCoordinator.ShowMessageAsync(this, "Confirm", $"Do you really want to empty Bin with No. #{bin.BinNumber}?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                await _binService.EmptyBinAsync(bin.BinNumber);
                await LoadBins();
            }
        }

        private DelegateCommand<Bin> _openInBrowserCommand;

        public DelegateCommand<Bin> OpenInBrowserCommand
        {
            get { return _openInBrowserCommand ??= new DelegateCommand<Bin>(HandleOpenInBrowwer); }
        }

        private void HandleOpenInBrowwer(Bin bin)
        {
            Task.Run(() => _browserService.NavigateToOrder(bin.OrderNumber));
            WindowHelper.FocusChrome();
        }

        private DelegateCommand<Bin> _editNotesCommand;

        public DelegateCommand<Bin> EditNotesCommand
        {
            get { return _editNotesCommand ??= new DelegateCommand<Bin>(HandleEditNotes); }
        }

        private async void HandleEditNotes(Bin bin)
        {
            if (bin.HasNotes)
            {
                var result = await _dialogCoordinator.ShowInputAsync(this, "Edit Notes", "Old Note:\n\n" + bin.Notes);
                if (string.IsNullOrEmpty(result))
                {
                    return;
                }


            }
        }

        public BinsViewModel(IDialogCoordinator dialogCoordinator, BinService binService, IShipStationBrowserService browserService, MessageBus messageBus)
        {
            _dialogCoordinator = dialogCoordinator;
            _binService = binService;
            _browserService = browserService;
            _messageBus = messageBus;

            Bins = CollectionViewSource.GetDefaultView(_bins);

            _messageBus.BinsDestroyed += _messageBus_BinsDestroyed;
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
                                    var bin = e as Bin;
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

        private void _messageBus_BinsDestroyed(object sender, int[] binNumbers)
        {
            foreach (var bin in _bins)
            {
                if (binNumbers.Contains(bin.BinNumber))
                {
                    _dispatcher.Invoke(() =>
                    {
                        _bins.Remove(bin);
                    });
                }
            }
        }

        private async Task LoadBins()
        {
            await _dispatcher.InvokeAsync(_bins.Clear);
            foreach (var bin in await _binService.ListBinsAsync())
            {
                await _dispatcher.InvokeAsync(() => _bins.Add(bin));
            }
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await LoadBins();
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
