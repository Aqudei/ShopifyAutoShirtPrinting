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

        public Main(IRegionManager regionManager, IDialogService dialogService)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;

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

                _regionManager.RequestNavigate("ContentRegion", "OrderProcessingView");
                HamburgerMenu.SelectedItem = HamburgerMenu.Items.OfType<MyMenuItem>()
                    .FirstOrDefault(x => x.NavigationPath == "OrderProcessingView");

                (DataContext as MainViewModel)?.LoadStores();
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
