using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using ShopifyEasyShirtPrinting.ViewModels;
using ShopifyEasyShirtPrinting.Views;
using ShopifySharp;
using ConfigurationBuilder = System.Configuration.ConfigurationBuilder;

namespace ShopifyAutoShirtPrinting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        // RegisterTypes function is here

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var shopUrl = Environment.GetEnvironmentVariable("SHOPIFY_SHOP_URL", EnvironmentVariableTarget.User);
            var shopToken = Environment.GetEnvironmentVariable("SHOPIFY_TOKEN", EnvironmentVariableTarget.User);
            Debug.WriteLine("Shop Url: {0}", shopUrl, null);
            Debug.WriteLine("Shop Token: {0}", shopToken, null);

            var orderService = new OrderService(shopUrl, shopToken);
            var productVariantService = new ProductVariantService(shopUrl, shopToken);
            var productImageService = new ProductImageService(shopUrl, shopToken);


            containerRegistry.RegisterDialog<ScannerView, ScannerViewModel>();
            containerRegistry.RegisterDialog<PrintQrView, PrintQrViewModel>();
            containerRegistry.RegisterDialogWindow<MetroDialogWindow>();

            containerRegistry.RegisterInstance(orderService);
            containerRegistry.RegisterInstance(productVariantService);
            containerRegistry.RegisterInstance(productImageService);
            containerRegistry.RegisterInstance(DialogCoordinator.Instance);

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("MainRegion", typeof(OrderProcessingView));
            regionManager.RegisterViewWithRegion("MainRegion", typeof(SettingsView));
            regionManager.RegisterViewWithRegion("MainRegion", typeof(OrganizerView));
            regionManager.RegisterViewWithRegion("ScanPrintRegion", typeof(ScanPrintView));
        }

        protected override Window CreateShell()
        {
            var shell = Container.Resolve<Shell>();

            return shell;
        }

        public App()
        {
            DotNetEnv.Env.Load();
        }
    }
}
