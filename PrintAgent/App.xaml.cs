using AutoMapper;
using Common.Api;
using Common.Models;
using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using OfficeOpenXml;
using PrintAgent.Models;
using PrintAgent.ViewModels;
using PrintAgent.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PrintAgent
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private readonly static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SessionVariables _globalVariables = new();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var dbPath = GetDbPath();
            containerRegistry.RegisterSingleton<ILiteDatabase>(() => new LiteDatabase(dbPath));
            var apiBaseUrl = !string.IsNullOrWhiteSpace(PrintAgent.Properties.Settings.Default.ApiBaseUrl) ? PrintAgent.Properties.Settings.Default.ApiBaseUrl : "https://workflows.louiestshirtprinting.co";
            containerRegistry.RegisterSingleton<SessionVariables>();
            containerRegistry.RegisterSingleton<ApiClient>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SettingsViewModel, PrintAgent.Properties.Settings>().ReverseMap();
                cfg.CreateMap<Common.Models.PrintRequest, Models.PrintRequest>().ReverseMap();
            });


            containerRegistry.RegisterSingleton<IMapper>(() => mapperConfig.CreateMapper());
            containerRegistry.RegisterSingleton<IRule, DefaultHFRule>();

            containerRegistry.RegisterInstance(DialogCoordinator.Instance);
        }

        private static string GetDbPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            var dataFolder = Path.Combine(localAppData, appName);
            Directory.CreateDirectory(dataFolder);

            var dbFile = Path.Combine(dataFolder, $"{appName.ToLower()}.db");
            return dbFile;
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Main>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            base.OnStartup(e);

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion<Printing>("MainRegion");
            regionManager.RegisterViewWithRegion<Settings>("MainRegion");
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "DispatcherUnhandledException");
        }
    }
}
