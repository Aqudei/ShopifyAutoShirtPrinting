using Common.Models;
using MahApps.Metro.Controls;
using Prism.Regions;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.ViewModels;
using System.Linq;
using System.Windows;
using MyMenuItem = ShopifyEasyShirtPrinting.ViewModels.MenuItem;

namespace ShopifyEasyShirtPrinting.Views
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main
    {
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
        private readonly SessionVariables _sessionVariables;

        public Main(IRegionManager regionManager, IDialogService dialogService, SessionVariables sessionVariables)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            _sessionVariables = sessionVariables;

            InitializeComponent();

            RegionManager.SetRegionManager(ContentRegion, regionManager);
            RegionManager.SetRegionName(ContentRegion, "ContentRegion");

            Loaded += Main_Loaded;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            _dialogService.ShowDialog("LoginDialog", r =>
            {
                if (r.Result != ButtonResult.OK)
                {
                    Application.Current.Shutdown();
                }

                (DataContext as MainViewModel)?.LoadStores();

                var defaultStore = _sessionVariables.Stores.FirstOrDefault(s => s.IsDefault);
                if (defaultStore != null)
                {
                    _regionManager.RequestNavigate("ContentRegion", "OrderProcessingView", new NavigationParameters { { "NavigationParam", defaultStore.Id } });
                    HamburgerMenu.SelectedItem = HamburgerMenu.Items.OfType<MyMenuItem>()
                                            .FirstOrDefault(x => x.NavigationPath == "OrderProcessingView" && x.NavigationParam == defaultStore);
                }
                else
                {
                    _regionManager.RequestNavigate("ContentRegion", "OrderProcessingView");

                    HamburgerMenu.SelectedItem = HamburgerMenu.Items.OfType<MyMenuItem>()
                            .FirstOrDefault(x => x.NavigationPath == "OrderProcessingView");
                }

            });

            HamburgerMenu.ItemInvoked += HamburgerMenu_ItemInvoked;
        }

        private void HamburgerMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            var item = args.InvokedItem as MyMenuItem;
            _regionManager.RequestNavigate("ContentRegion", item.NavigationPath, new NavigationParameters() { { "NavigationParam", item.NavigationParam } });

        }
    }
}
