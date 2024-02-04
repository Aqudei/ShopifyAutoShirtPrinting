using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ShopifyEasyShirtPrinting.Views;
using ShopifyEasyShirtPrinting.Views.Tools;
using System;
using System.Collections.ObjectModel;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class MainViewModel : PageBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override string Title
        {
            get
            {
                if (AppDomain.CurrentDomain.BaseDirectory.ToLower().Contains("staging"))
                {
                    return "TLKC Tooling (Staging)";
                }

                return "TLKC Tooling (Stable)";
            }
        }

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

            Menu.Add(new MenuItem
            {
                Label = "Tooling",
                NavigationPath = "Tooling",
                NavigationType = typeof(Tooling),
                Icon = new PackIconBootstrapIcons
                {
                    Kind = PackIconBootstrapIconsKind.Tools
                }
            });

            Menu.Add(new MenuItem
            {
                Label = "Archived Items",
                NavigationPath = "Archived",
                NavigationType = typeof(Archived),
                Icon = new PackIconBootstrapIcons
                {
                    Kind = PackIconBootstrapIconsKind.Archive
                }
            });

            Menu.Add(new MenuItem
            {
                Label = "Products",
                NavigationPath = "Products",
                NavigationType = typeof(Products),
                Icon = new PackIconMaterial
                {
                    Kind = PackIconMaterialKind.TshirtV
                }
            });

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