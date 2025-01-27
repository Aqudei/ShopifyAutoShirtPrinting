using Common.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ShopifyEasyShirtPrinting.Views;
using ShopifyEasyShirtPrinting.Views.Tools;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class MainViewModel : PageBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SessionVariables _sessionVariables;

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

        private BackgroundWorker _backGroundTaskRunner = new BackgroundWorker();

        public MainViewModel(SessionVariables sessionVariables)
        {
            //Menu.Add(new MenuItem
            //{
            //    Label = "LKC Orders",
            //    NavigationPath = "OrderProcessingView",
            //    NavigationType = typeof(OrderProcessingView),
            //    Icon = new PackIconFontAwesome
            //    {
            //        Kind = PackIconFontAwesomeKind.FirstOrderAltBrands
            //    }
            //});

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

            Menu.Add(new MenuItem
            {
                Label = "Shipments",
                NavigationPath = "ShipmentsView",
                NavigationType = typeof(ShipmentsView),
                Icon = new PackIconMaterial
                {
                    Kind = PackIconMaterialKind.ShippingPallet
                }
            });

            Menu.Add(new MenuItem
            {
                Label = "Archived Shipments",
                NavigationPath = "ArchivedShipmentsView",
                NavigationType = typeof(ArchivedShipmentsView),
                Icon = new PackIconMaterial
                {
                    Kind = PackIconMaterialKind.ShippingPallet
                }
            });

            Menu.Add(new MenuItem
            {
                Label = "Harmonization",
                NavigationPath = "Harmonization",
                NavigationType = typeof(Harmonization),
                Icon = new PackIconMaterial
                {
                    Kind = PackIconMaterialKind.CodeString
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


            OptionsMenu.Add(new MenuItem
            {
                Label = "Tasks",
                NavigationPath = "TasksView",
                NavigationType = typeof(TasksView),
                Icon = new PackIconFontAwesome
                {
                    Kind = PackIconFontAwesomeKind.TasksSolid
                }
            });

            _sessionVariables = sessionVariables;
        }

        public void LoadStores()
        {
            foreach (var store in _sessionVariables.Stores?.OrderBy(s => s.IsDefault))
            {
                Menu.Insert(0,new MenuItem
                {
                    Label = store.Name,
                    NavigationPath = "OrderProcessingView",
                    NavigationType = typeof(OrderProcessingView),
                    NavigationParam = store,
                    Icon = new PackIconFontAwesome
                    {
                        Kind = PackIconFontAwesomeKind.ShoppingBagSolid
                    }
                });
            }
        }
    }

    public class MenuItem : HamburgerMenuIconItem
    {
        public string NavigationPath { get; internal set; }
        public Type NavigationType { get; internal set; }
        public object NavigationParam { get; internal set; }
    }
}