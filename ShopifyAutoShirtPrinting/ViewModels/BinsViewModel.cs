using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class BinsViewModel : PageBase, INavigationAware
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly BinService _binService;
        private DelegateCommand<Bin> clearBinCommand;

        public override string Title => "Bins";
        public ObservableCollection<Bin> Bins { get; set; } = new();

        public DelegateCommand<Bin> ClearBinCommand { get => clearBinCommand ??= new DelegateCommand<Bin>(HandleClearBin); }

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
        }

        private void LoadBins()
        {
            Bins.Clear();
            Bins.AddRange(_binService.ListBins().ToList());
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
