using AutoMapper;
using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using OfficeOpenXml;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.Services;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using ShopifyEasyShirtPrinting.ViewModels;
using ShopifyEasyShirtPrinting.ViewModels.Dialogs;
using ShopifyEasyShirtPrinting.ViewModels.Tools;
using ShopifyEasyShirtPrinting.Views;
using ShopifyEasyShirtPrinting.Views.Dialogs;
using ShopifyEasyShirtPrinting.Views.Tools;
using System;
using System.IO;
using System.Windows;
using DotNetEnv;

namespace ShopifyEasyShirtPrinting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        // RegisterTypes function is here
        private readonly SessionVariables _sessionVariables = new();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            try
            {

                containerRegistry.RegisterInstance(_sessionVariables);
                // containerRegistry.RegisterDialog<PrintQrView, PrintQrViewModel>();
                containerRegistry.RegisterDialog<CrudDialog, CrudDialogViewModel>();
                containerRegistry.RegisterDialog<EditTextDialog, EditTextDialogViewModel>();
                containerRegistry.RegisterDialog<HarmonizationDialog, HarmonizationDialogViewModel>();
                containerRegistry.RegisterDialog<Views.Dialogs.LoginDialog, LoginDialogViewModel>();
                containerRegistry.RegisterDialogWindow<MetroDialogWindow>();
                containerRegistry.Register<MyPrintService>();
                containerRegistry.Register<BinService>();

                containerRegistry.Register<ApiClient>();

                var databaseHost = string.IsNullOrWhiteSpace(Settings.Default.ServerHost) ? "localhost" : Settings.Default.ServerHost;
                var databasePort = Settings.Default.ServerPort;
                var databaseName = Settings.Default.DatabaseName;
                var databaseUser = Settings.Default.DatabaseUser;
                var databasePass = Settings.Default.DatabasePass;

                containerRegistry.RegisterInstance(DialogCoordinator.Instance);

                var regionManager = Container.Resolve<IRegionManager>();

                containerRegistry.RegisterForNavigation<OrderProcessingView>("OrderProcessingView");
                containerRegistry.RegisterForNavigation<Bins>();
                containerRegistry.RegisterForNavigation<SettingsView>();
                containerRegistry.RegisterForNavigation<Tooling>();
                containerRegistry.RegisterForNavigation<Archived>();
                containerRegistry.RegisterForNavigation<Products>();
                containerRegistry.RegisterForNavigation<ShipmentsView>();
                containerRegistry.RegisterForNavigation<ArchivedShipmentsView>();
                containerRegistry.RegisterForNavigation<TasksView>();
                containerRegistry.RegisterForNavigation<Harmonization>();

                containerRegistry.RegisterInstance<IMessageBus>(new DummyMessageBus());

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<MyLineItem, MyLineItem>();
                    cfg.CreateMap<MyLineItem, LineItemViewModel>()
                    .ReverseMap();
                    cfg.CreateMap<LineItemViewModel, LineItemViewModel>();
                    cfg.CreateMap<MyLineItem, CrudDialogViewModel>()
                    .ReverseMap();
                    cfg.CreateMap<OrderInfo, OrderInfo>();
                    cfg.CreateMap<Log, Log>();
                    cfg.CreateMap<BinViewModel, Bin>()
                    .ReverseMap();

                    cfg.CreateMap<Settings, SettingsViewModel>()
                    .ReverseMap();

                    cfg.CreateMap<LoginDialogViewModel.SessionInfo, SessionVariables>();
                    cfg.CreateMap<LoginDialogViewModel.LoginResultBody, SessionVariables>();
                    cfg.CreateMap<Common.Models.Shipment, LabelPrintingDialogViewModel>();

                    cfg.CreateMap<LabelPrintingDialogViewModel, CreateShipmentRequestBody>();
                    cfg.CreateMap<Common.Models.Shipment, Models.Shipment>().ReverseMap();
                    cfg.CreateMap<Common.Models.Harmonisation.HSN, Models.Harmonisation.HSN>().ReverseMap();


                });

                containerRegistry.RegisterInstance(config.CreateMapper());
                containerRegistry.RegisterDialog<LabelPrintingDialog, LabelPrintingDialogViewModel>();
                containerRegistry.RegisterDialog<AfterScanDialog, AfterScanDialogViewModel>();
                containerRegistry.RegisterDialog<QuantityChangerDialog, QuantityChangerDialogViewModel>();


                containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();


                regionManager.RegisterViewWithRegion<SkuTool>("ToolsRegion");
                regionManager.RegisterViewWithRegion<SizingTool>("ToolsRegion");
                // regionManager.RegisterViewWithRegion<Harmonization>("ToolsRegion");

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
            var exception = e.Exception;

            var errorMessage = exception.Message + Environment.NewLine;

            while (exception.InnerException != null)
            {
                errorMessage += exception.InnerException.Message + Environment.NewLine;
                exception = exception.InnerException;
            }

            MessageBox.Show($"{errorMessage}\n\n{exception.StackTrace}");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Env.Load();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Required for EPPlus 5 and above

            Logger.Debug("Program Started!");

            var appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);

            _sessionVariables.DataPath = dataPath;
            _sessionVariables.PdfsPath = Path.Combine(dataPath, "PDFs");
            _sessionVariables.QrTagsPath = Path.Combine(dataPath, "QRs");
            _sessionVariables.ImagesPath = Path.Combine(dataPath, "Images");
            _sessionVariables.DbName = Path.Combine(_sessionVariables.DataPath, $"{appName}.db");

            Directory.CreateDirectory(_sessionVariables.DataPath);
            Directory.CreateDirectory(_sessionVariables.PdfsPath);
            Directory.CreateDirectory(_sessionVariables.QrTagsPath);
            Directory.CreateDirectory(_sessionVariables.ImagesPath);

            base.OnStartup(e);
        }
    }
}
