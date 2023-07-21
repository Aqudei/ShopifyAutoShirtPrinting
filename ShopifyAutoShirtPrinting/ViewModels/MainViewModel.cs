using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ShopifyEasyShirtPrinting.Views;
using System;
using System.Collections.ObjectModel;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class MainViewModel : PageBase
    {
        public override string Title => "Main";


        public ObservableCollection<MenuItem> Menu { get; set; } = new();
        public ObservableCollection<MenuItem> OptionsMenu { get; set; } = new();

        public MainViewModel()
        {
            Menu.Add(new MenuItem
            {
                Label = "Order Processing",
                NavigationPath = "OrderProcessingView",
                NavigationType = typeof(OrderProcessingView),
                Icon = new PackIconFontAwesome
                {
                    Kind = PackIconFontAwesomeKind.FirstOrderAltBrands
                }
            });

            Menu.Add(new MenuItem
            {
                Label = "Bins",
                NavigationPath = "Bins",
                NavigationType = typeof(Bins),
                Icon = new PackIconBootstrapIcons
                {
                    Kind = PackIconBootstrapIconsKind.Basket
                }
            });



            //Menu.Add(new MenuItem
            //{
            //    Label = "Shipped",
            //    NavigationPath = "ShippedView",
            //    NavigationType = typeof(ShippedView),
            //    Icon = new PackIconFontAwesome
            //    {
            //        Kind = PackIconFontAwesomeKind.ShipSolid
            //    }
            //});


            OptionsMenu.Add(new MenuItem
            {
                Label = "Settings",
                NavigationPath = "SettingsView",
                NavigationType = typeof(SettingsView),
                Icon = new PackIconFontAwesome
                {
                    Kind = PackIconFontAwesomeKind.ToolboxSolid
                }
            });
        }
    }

    public class MenuItem : HamburgerMenuIconItem
    {
        public string NavigationPath { get; internal set; }
        public Type NavigationType { get; internal set; }
    }
}