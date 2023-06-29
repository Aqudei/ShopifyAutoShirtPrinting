using AutoMapper;
using DotNetEnv;
using EasyNetQ;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Data;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.Services;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using ShopifyEasyShirtPrinting.ViewModels;
using ShopifyEasyShirtPrinting.ViewModels.Dialogs;
using ShopifyEasyShirtPrinting.Views;
using ShopifyEasyShirtPrinting.Views.Dialogs;
using ShopifySharp;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ShopifyEasyShirtPrinting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        // RegisterTypes function is here
        readonly GlobalVariables _globalVariables = new();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            try
            {
                // Process.Start("explorer.exe", $"\"{dataPath}\"");

                Env.Load();

                var shopUrl = Environment.GetEnvironmentVariable("SHOPIFY_SHOP_URL", EnvironmentVariableTarget.User);
                var shopToken = Environment.GetEnvironmentVariable("SHOPIFY_TOKEN", EnvironmentVariableTarget.User);
                _globalVariables.ShopUrl = shopUrl;
                _globalVariables.ShopToken = shopToken;

                Debug.WriteLine($"Shop Url: {shopUrl}");
                Debug.WriteLine($"Shop Token: {shopToken}");

                var orderService = new OrderService(shopUrl, shopToken);
                var productVariantService = new ProductVariantService(shopUrl, shopToken);
                var productImageService = new ProductImageService(shopUrl, shopToken);

                containerRegistry.RegisterDialog<PrintQrView, PrintQrViewModel>();
                containerRegistry.RegisterDialogWindow<MetroDialogWindow>();
                containerRegistry.Register<ShipStationApi>();
                containerRegistry.Register<MyPrintService>();
                containerRegistry.Register<BinService>();
                containerRegistry.RegisterInstance(orderService);
                containerRegistry.RegisterInstance(productVariantService);
                containerRegistry.RegisterInstance(productImageService);


                var databaseHost = string.IsNullOrWhiteSpace(Settings.Default.DatabaseHost) ? "localhost" : Settings.Default.DatabaseHost;
                var databasePort = Settings.Default.DatabasePort;
                var databaseName = Settings.Default.DatabaseName;
                var databaseUser = Settings.Default.DatabaseUser;
                var databasePass = Settings.Default.DatabasePass;

                var connectionString = "";

                //if (System.Environment.MachineName.Contains("LAPTOP-DB8A9BOL"))
                //    connectionString = $"Server=localhost;Port=5432;Database=thelonelykids;User Id=postgres;Password=Espelimbergo;";
                //else
                    connectionString = $"Server={databaseHost};Port={databasePort};Database={databaseName};User Id={databaseUser};Password={databasePass};";

                Database.SetInitializer(new MigrateDatabaseToLatestVersion<LonelyKidsContext, Migrations.Configuration>(useSuppliedContext: true));
                containerRegistry.RegisterInstance(new LonelyKidsContext(connectionString));
                containerRegistry.RegisterInstance(new DbService(connectionString));

                containerRegistry.RegisterInstance(DialogCoordinator.Instance);
                containerRegistry.RegisterInstance(_globalVariables);

                var regionManager = Container.Resolve<IRegionManager>();

                containerRegistry.RegisterForNavigation<OrderProcessingView>();
                containerRegistry.RegisterForNavigation<ShippedView>();
                containerRegistry.RegisterForNavigation<Bins>();
                containerRegistry.RegisterForNavigation<SettingsView>();

                var bus = RabbitHutch.CreateBus($"host={Settings.Default.DatabaseHost};username=warwick;password=warwickpass1");
                
                containerRegistry.RegisterInstance(bus);


                // regionManager.RegisterViewWithRegion("MainRegion", typeof(OrganizerView));

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<MyLineItem, MyLineItem>();
                    cfg.CreateMap<OrderInfo, OrderInfo>();
                    cfg.CreateMap<Log, Log>();
                    cfg.CreateMap<Settings, SettingsViewModel>().ReverseMap();
                });
                containerRegistry.RegisterInstance(config.CreateMapper());
                containerRegistry.RegisterDialog<LabelPrintingDialog, LabelPrintingDialogViewModel>();
                containerRegistry.RegisterDialog<AfterScanDialog, AfterScanDialogViewModel>();


                containerRegistry.RegisterSingleton<ILineRepository, LineRepository>();
                containerRegistry.RegisterSingleton<IOrderRepository, OrderRepository>();
                containerRegistry.RegisterSingleton<LogRespository>();



                containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();


                if (Settings.Default.UseBrowser)
                {
                    containerRegistry.RegisterSingleton<IShipStationBrowserService, ShipStationBrowserService>();
                }
                else
                {
                    containerRegistry.RegisterSingleton<IShipStationBrowserService, DummyShipstatiionBrowserService>();
                }
                Container.Resolve<IShipStationBrowserService>().DoLogin();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        protected override Window CreateShell()
        {
            var shell = Container.Resolve<Main>();
            return shell;
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception);
            MessageBox.Show($"{e.Exception.Message}\n{e.Exception.StackTrace}");
        }


        private void PrismApplication_Startup(object sender, StartupEventArgs e)
        {
            Logger.Debug("Program Started!");

            var appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);

            _globalVariables.DataPath = dataPath;
            _globalVariables.PdfsPath = Path.Combine(dataPath, "PDFs");
            _globalVariables.QrTagsPath = Path.Combine(dataPath, "QRs");
            _globalVariables.ImagesPath = Path.Combine(dataPath, "Images");
            _globalVariables.DbName = Path.Combine(_globalVariables.DataPath, $"{appName}.db");

            Directory.CreateDirectory(_globalVariables.DataPath);
            Directory.CreateDirectory(_globalVariables.PdfsPath);
            Directory.CreateDirectory(_globalVariables.QrTagsPath);
            Directory.CreateDirectory(_globalVariables.ImagesPath);
        }

        private void PrismApplication_Exit(object sender, ExitEventArgs e)
        {
            if (Container.IsRegistered<IShipStationBrowserService>())
                Container.Resolve<IShipStationBrowserService>().Dispose();

            if (Container.IsRegistered<IBus>())
            {
                Container.Resolve<IBus>().Dispose();
            }
        }
    }
}
