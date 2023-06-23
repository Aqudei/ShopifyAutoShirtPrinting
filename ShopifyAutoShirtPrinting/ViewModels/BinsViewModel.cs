using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class BinsViewModel : PageBase, INavigationAware
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
                _binService.EmptyBin(bin.BinNumber);
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
                                    var nameMatched = bin.Items.Any(x => x.Name.ToLower().Contains(SearchText.ToLower()));
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

        private void LoadBins()
        {
            _bins.Clear();
            _bins.AddRange(_binService.ListBins().ToList());
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadBins();
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
