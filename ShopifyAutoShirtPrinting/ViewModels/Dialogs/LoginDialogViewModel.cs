using AutoMapper;
using Common.Api;
using Common.Models;
using DryIoc;
using LiteDB;
using Netco.Extensions;
using NLog;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RestSharp;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class LoginDialogViewModel : BindableBase, IDialogAware
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public class LoginResultBody
        {
            [JsonPropertyName("access")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh")]
            public string RefreshToken { get; set; }
        }

        public class SessionInfo
        {
            [JsonPropertyName("logging_email")]
            public string LoggingEmail { get; set; }
            [JsonPropertyName("logging_password")]
            public string LoggingPassword { get; set; }
            [JsonPropertyName("broadcast_exchange")]
            public string BroadcastExchange { get; set; }
            [JsonPropertyName("broadcast_username")]
            public string BroadcastUsername { get; set; }
            [JsonPropertyName("broadcast_password")]
            public string BroadcastPassword { get; set; }
            [JsonPropertyName("broadcast_host")]
            public string BroadcastHost { get; set; }
            [JsonPropertyName("shipstation_username")]
            public string ShipStationUsername { get; set; }
            [JsonPropertyName("shipstation_password")]
            public string ShipStationPassword { get; set; }
        }

        public class Server
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }

        }

        private readonly IMapper _mapper;
        private readonly SessionVariables _sessionVariables;
        private readonly IContainerExtension _container;
        private readonly string _dbPath;
        private DelegateCommand loginCommand;
        private bool _isBusy = false;
        public bool CanLogin => !IsBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetProperty(ref _isBusy, value);
                RaisePropertyChanged(nameof(CanLogin));
            }
        }

        public LoginResultBody LoginResult { get; set; }

        public string Title => "Login";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public LoginDialogViewModel(IMapper mapper, SessionVariables sessionVariables,
            IContainerExtension container)
        {
            _mapper = mapper;
            _sessionVariables = sessionVariables;
            _container = container;
            _dbPath = System.IO.Path.Combine(_sessionVariables.DataPath, "local_data.db");


            LoadServers();

            SelectedServer = Servers.FirstOrDefault()?.Url;

            PropertyChanged += LoginDialogViewModel_PropertyChanged;
        }

        private void LoginDialogViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserName))
            {
                ErrorMessage = "";
            }
        }

        public ObservableCollection<Server> Servers { get; set; } = new();
        private string _errorMessage;
        private string _userName;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        private void LoadServers()
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                db.GetCollection<Server>().FindAll().ForEach(Servers.Add);
            }

            var defaultServerUrl = "https://workflows.louiestshirtprinting.co";
            if (!Servers.Any(s => s.Url.Contains(defaultServerUrl)))
            {
                using (var db = new LiteDatabase(_dbPath))
                {
                    var defaultServer = new Server
                    {
                        Url = defaultServerUrl
                    };
                    db.GetCollection<Server>().Insert(defaultServer);
                    Servers.Add(defaultServer);
                }
            }
        }

        public string SelectedServer { get; set; }

        public SecureString Password { get; set; }
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        public DelegateCommand LoginCommand { get => loginCommand ??= new DelegateCommand(OnLogin); }
        public static string ConvertToUnsecureString(SecureString secureString)
        {
            if (secureString == null)
                return string.Empty;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private void SetupEmailErrorLogging(string loggingEmail, string loggingPassword)
        {
            try
            {
                Environment.SetEnvironmentVariable("GMAIL_USERNAME", loggingEmail);
                Environment.SetEnvironmentVariable("GMAIL_PASSWORD", loggingPassword);
                LogManager.ReconfigExistingLoggers();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async void OnLogin()
        {
            try
            {
                IsBusy = true;

                if (!Servers.Any(s => s.Url == SelectedServer))
                {
                    var newServerUrl = SelectedServer;

                    using (var db = new LiteDatabase(_dbPath))
                    {
                        var newServer = new Server
                        {
                            Url = newServerUrl
                        };
                        db.GetCollection<Server>().Insert(newServer);
                        Servers.Add(newServer);
                    }
                }

                var password = ConvertToUnsecureString(Password);
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(UserName))
                {
                    ErrorMessage = "Login Failed. Please provide username and password.";
                    return;
                }

                using (var db = new LiteDatabase(_dbPath))
                {
                    var client = new RestClient(SelectedServer);

                    var request = new RestRequest("/api/token/")
                    .AddParameter("username", UserName)
                        .AddParameter("password", password);
                    var authResponse = await client.ExecutePostAsync<LoginResultBody>(request);
                    if (!authResponse.IsSuccessStatusCode)
                    {
                        ErrorMessage = $"Login Failed. {authResponse.ErrorMessage ?? authResponse.Content} ";
                        return;
                    }

                    LoginResult = authResponse.Data;

                    request = new RestRequest("/api/accounts/session")
                        .AddHeader("Authorization", $"Bearer {authResponse.Data.AccessToken}");

                    var sessionResponse = await client.ExecuteGetAsync<SessionInfo>(request);
                    if (!sessionResponse.IsSuccessStatusCode)
                        return;

                    _sessionVariables.ServerUrl = SelectedServer;
                    _mapper.Map(authResponse.Data, _sessionVariables);
                    _mapper.Map(sessionResponse.Data, _sessionVariables);

                    SetupEmailErrorLogging(sessionResponse.Data.LoggingEmail, sessionResponse.Data.LoggingPassword);
                    SetupMessageBus();
                    await SetupShipStationBrowser();

                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SetupShipStationBrowser()
        {
            if (Settings.Default.UseBrowser)
            {
                _container.RegisterSingleton<IShipStationBrowserService, ShipStationBrowserService>();
            }

            else
            {
                _container.RegisterSingleton<IShipStationBrowserService, DummyShipstatiionBrowserService>();
            }

            await Task.Run(() => _container.Resolve<IShipStationBrowserService>().DoLogin());
        }

        private void SetupMessageBus()
        {
            _container.RegisterSingleton<IMessageBus, MessageBus>();
            System.Diagnostics.Debug.WriteLine("Bus Registered!");
        }
    }
}
