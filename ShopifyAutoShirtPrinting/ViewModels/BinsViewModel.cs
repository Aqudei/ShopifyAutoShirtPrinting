using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Services;
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
                _binService.EmptyBinAsync(bin.BinNumber);
                LoadBins();
            }
        }

        public BinsViewModel(IDialogCoordinator dialogCoordinator, BinService binService)
        {
            _dialogCoordinator = dialogCoordinator;
            _binService = binService;

            Bins = CollectionViewSource.GetDefaultView(_bins);


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
