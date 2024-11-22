using Common.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using ShopifyEasyShirtPrinting.Views;
using ShopifyEasyShirtPrinting.Views.Tools;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class MainViewModel : PageBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SessionVariables _globalVariables;

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

        public MainViewModel(SessionVariables globalVariables)
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

            _globalVariables = globalVariables;

            _backGroundTaskRunner.DoWork += _backGroundTaskRunner_DoWork;
            _backGroundTaskRunner.WorkerReportsProgress = true;
            _backGroundTaskRunner.RunWorkerAsync();

        }

        private void _backGroundTaskRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (_globalVariables.TaskQueue.TryDequeue(out var task))
                {
                    try
                    {
                        task.Execute();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

                Thread.Sleep(100);
            }
        }
    }

    public class MenuItem : HamburgerMenuIconItem
    {
        public string NavigationPath { get; internal set; }
        public Type NavigationType { get; internal set; }
    }
}