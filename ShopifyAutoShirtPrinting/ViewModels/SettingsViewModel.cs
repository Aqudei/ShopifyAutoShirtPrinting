using AutoMapper;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Printing;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class SettingsViewModel : PageBase
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IMapper _mapper;

        public ObservableCollection<string> QrPrinters { get; set; } = new();
        public ObservableCollection<string> LabelPrinters { get; set; } = new();

        public string QrPrinter { get => qrPrinter; set => SetProperty(ref qrPrinter, value); }
        public string LabelPrinter { get => labelPrinter; set => SetProperty(ref labelPrinter, value); }
        public string HotFoldersConfig { get => hotFoldersConfig; set => SetProperty(ref hotFoldersConfig, value); }

        public ObservableCollection<string> Tags { get; set; } = new ObservableCollection<string>();
        public DelegateCommand<string> TagsCommand => _addTagCommand ??= new DelegateCommand<string>(HandleAddTag);

        private async void HandleAddTag(string cmd)
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

        public DelegateCommand ResetDatabaseCommand
        {
            get { return _resetDatabaseCommand ??= new DelegateCommand(HandleResetDatabase); }
        }

        private async void HandleResetDatabase()
        {
            var prompt = await _dialogCoordinator.ShowMessageAsync(this, "Confirm Database Reset",
                "Are you sure you want to cleanup database?", MessageDialogStyle.AffirmativeAndNegative);

            if (prompt != MessageDialogResult.Affirmative)
                return;

            await _apiClient.ResetDatabase();
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(3));
        }


        public override string Title => "Settings";
        public SettingsViewModel(IDialogCoordinator dialogCoordinator, IMapper mapper, ApiClient apiClient)
        {
            _mapper = mapper;
            _apiClient = apiClient;
            _dialogCoordinator = dialogCoordinator;
            
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                QrPrinters.Add(printerName);
            }

            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                LabelPrinters.Add(printerName);
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

        public double PaperWidth { get => paperWidth; set => SetProperty(ref paperWidth, value); }
        public double PaperHeight { get => paperHeight; set => SetProperty(ref paperHeight, value); }
        public bool UseBrowser { get => useBrowser; set => SetProperty(ref useBrowser, value); }
        public int FontSize { get => fontSize; set => SetProperty(ref fontSize, value); }


        private DelegateCommand _saveSettingsCommand;
        private double paperWidth;
        private double paperHeight;
        private int fontSize;
        private string qrPrinter;
        private string labelPrinter;
        private string hotFoldersConfig;
        private bool useBrowser;
        private DelegateCommand<string> _addTagCommand;
#pragma warning disable CS0169 // The field 'SettingsViewModel._tagInput' is never used
        private string _tagInput;
#pragma warning restore CS0169 // The field 'SettingsViewModel._tagInput' is never used
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

        private async void HandleSaveSettings()
        {
            _mapper.Map(this, Settings.Default);
            Settings.Default.Save();
            await _dialogCoordinator.ShowMessageAsync(this, "Success", "Settings successfully saved.");
        }
    }
}
