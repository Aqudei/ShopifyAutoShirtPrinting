using AutoMapper;
using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class SettingsViewModel : PageBase
    {
        private readonly ApiClient _apiClient;
        private readonly SessionVariables _globalVariables;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IMapper _mapper;

        public ObservableCollection<string> Printers { get; set; } = new();

        public string DownloadedImagesPath { get => _downloadedImagesPath; set => SetProperty(ref _downloadedImagesPath, value); }
        public string QrPrinter { get => qrPrinter; set => SetProperty(ref qrPrinter, value); }
        public string ManifestPrinter { get => _manifestPrinter; set => SetProperty(ref _manifestPrinter, value); }
        public string LabelPrinter { get => _labelPrinter; set => SetProperty(ref _labelPrinter, value); }
        private string _internationalLabelPrinter;

        public string InternationalLabelPrinter
        {
            get { return _internationalLabelPrinter; }
            set { SetProperty(ref _internationalLabelPrinter, value); }
        }

        public string HotFoldersConfig { get => hotFoldersConfig; set => SetProperty(ref hotFoldersConfig, value); }

        private string _printFilesFolder;

        public string PrintFilesFolder
        {
            get => _printFilesFolder;
            set => SetProperty(ref _printFilesFolder, value);
        }

        public ObservableCollection<string> Tags { get; set; } = new ObservableCollection<string>();
        public DelegateCommand<string> TagsCommand => _addTagCommand ??= new DelegateCommand<string>(OnAddTag);
        private DelegateCommand _cleanDownloadedImagesCommand;

        public DelegateCommand CleanDownloadedImagesCommand
        {
            get { return _cleanDownloadedImagesCommand ??= new DelegateCommand(OnCleanDownloadedImages); }
        }

        private async void OnCleanDownloadedImages()
        {
            var response = await _dialogCoordinator.ShowMessageAsync(this, "Confirm Delete Images",
                $"Deleting downloaded images from {_globalVariables.ImagesPath} will let you save disk space. Do you want to proceed?", MessageDialogStyle.AffirmativeAndNegative);
            if (response != MessageDialogResult.Affirmative)
            {
                return;
            }

            try
            {
                Directory.Delete(_globalVariables.ImagesPath, true);
                Directory.CreateDirectory(_globalVariables.ImagesPath);
                CalculateImagesUsageSize();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async void OnAddTag(string cmd)
        {
            if (cmd == "ADD")
            {
                var tag = await _dialogCoordinator.ShowInputAsync(this, "Input", "Tag Name");
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    Tags.Add(tag);
                    SaveSettingsCommand.Execute();
                }
            }

            if (cmd == "CLEAR")
            {
                Tags.Clear();
                SaveSettingsCommand.Execute();
            }

        }

        private string _serverHost = "170.64.158.123";

        public string ServerHost
        {
            get { return _serverHost; }
            set { SetProperty(ref _serverHost, value); }
        }

        private DelegateCommand _resetDatabaseCommand;

        public DelegateCommand ResetDatabaseCommand => _resetDatabaseCommand ??= new DelegateCommand(HandleResetDatabase);

        private async void HandleResetDatabase()
        {
            var prompt = await _dialogCoordinator.ShowMessageAsync(this, "Confirm Database Reset",
                "Are you sure you want to cleanup database?", MessageDialogStyle.AffirmativeAndNegative);

            if (prompt != MessageDialogResult.Affirmative)
                return;

            await _apiClient.ResetDatabase();
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(3));
        }

        private string _imagesTotalSize;

        public string ImagesTotalSize
        {
            get { return _imagesTotalSize; }
            set { SetProperty(ref _imagesTotalSize, value); }
        }

        public override string Title => "Settings";
        public SettingsViewModel(IDialogCoordinator dialogCoordinator, IMapper mapper, ApiClient apiClient, SessionVariables globalVariables)
        {
            _mapper = mapper;
            _apiClient = apiClient;
            _globalVariables = globalVariables;
            _dialogCoordinator = dialogCoordinator;

            CalculateImagesUsageSize();

            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                Printers.Add(printerName);
            }

            _mapper.Map(Settings.Default, this);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ServerHost))
                {
                    ApiBaseUrl = $"https://{ServerHost}";
                }
            };
        }

        private void CalculateImagesUsageSize()
        {
            try
            {
                DownloadedImagesPath = _globalVariables.ImagesPath;
                ImagesTotalSize = $"{DirectoryHelper.FormatSize(DirectoryHelper.GetDirectorySize(DownloadedImagesPath))} in used.";
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public double PaperWidth { get => paperWidth; set => SetProperty(ref paperWidth, value); }
        public double PaperHeight { get => paperHeight; set => SetProperty(ref paperHeight, value); }
        public bool UseBrowser { get => useBrowser; set => SetProperty(ref useBrowser, value); }
        public int FontSize { get => fontSize; set => SetProperty(ref fontSize, value); }


        private DelegateCommand _saveSettingsCommand;
        private double paperWidth;
        private double paperHeight;
        private int fontSize;
        private string qrPrinter;
        private string _labelPrinter;
        private string hotFoldersConfig;
        private bool useBrowser;
        private DelegateCommand<string> _addTagCommand;

        private string _serverPort;
        private string databasePass;
        private string databaseUser;
        private string databaseName;
        private string apiBaseUrl;

        public string ServerPort { get => _serverPort; set => SetProperty(ref _serverPort, value); }
        public string DatabasePass { get => databasePass; set => SetProperty(ref databasePass, value); }
        public string DatabaseUser { get => databaseUser; set => SetProperty(ref databaseUser, value); }
        public string DatabaseName { get => databaseName; set => SetProperty(ref databaseName, value); }
        public string ApiBaseUrl { get => apiBaseUrl; set => SetProperty(ref apiBaseUrl, value); }

        public DelegateCommand SaveSettingsCommand
        {
            get { return _saveSettingsCommand ??= new DelegateCommand(HandleSaveSettings); }
        }

        private DelegateCommand _browsePrintFilesLocationCommand;
        private string _downloadedImagesPath;
        private string _manifestPrinter;

        public DelegateCommand BrowsePrintFilesLocationCommand => _browsePrintFilesLocationCommand ??= new DelegateCommand(HandleBrowsePrintFiles);

        private void HandleBrowsePrintFiles()
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true,
            };

            var dlgResult = dialog.ShowDialog();
            if (dlgResult != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return;
            }

            PrintFilesFolder = dialog.FileName;
        }

        private async void HandleSaveSettings()
        {
            _mapper.Map(this, Settings.Default);
            Settings.Default.Save();
            await _dialogCoordinator.ShowMessageAsync(this, "Success", "Settings successfully saved.");
        }



    }
}
