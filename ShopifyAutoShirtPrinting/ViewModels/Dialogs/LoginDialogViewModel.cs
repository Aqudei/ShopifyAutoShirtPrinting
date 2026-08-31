using AutoMapper;
using Common.Api;
using Common.Models;
using DryIoc;
using LiteDB;
using NLog;
using Prism.Commands;
using Prism.Container.DryIoc;
using Prism.Dialogs;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using RestSharp;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
            [JsonPropertyName("stores")]
            public IEnumerable<Store> Stores { get; set; }
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
        private readonly Dispatcher _dispatcher;
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

            _dispatcher = Application.Current.Dispatcher;
            LoadServers();

            SelectedServer = Servers.FirstOrDefault()?.Url;

            PropertyChanged += LoginDialogViewModel_PropertyChanged;
        }

        private void LoginDialogViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserName))
            {
                Message = null;
            }
        }

        public ObservableCollection<Server> Servers { get; set; } = new();
        private DisplayMessage _message;
        private string _userName;

        public DisplayMessage Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private DelegateCommand _deleteServerCommand;
        private string _selectedServer;
        private DialogCloseListener _requestClose;

        public DelegateCommand DeleteServerCommand
        {
            get
            {
                return _deleteServerCommand ??= new DelegateCommand(OnDeleteServer, () => SelectedServer != null)
                    .ObservesProperty(() => SelectedServer);
            }
        }

        private void OnDeleteServer()
        {
            Server server = null;
            using (var db = new LiteDatabase(_dbPath))
            {
                var servers = db.GetCollection<Server>().FindAll().ToList();
                if (!string.IsNullOrWhiteSpace(SelectedServer))
                {
                    server = servers.FirstOrDefault(s => s.Url == SelectedServer);
                }
                else
                {
                    server = servers.FirstOrDefault(s => s.Url == "" || s.Url == null);
                }

                if (server != null)
                {
                    db.GetCollection<Server>().Delete(server.Id);
                    var toRemove = Servers.FirstOrDefault(ss => server.Url == server.Url);
                    if (toRemove != null)
                    {
                        Servers.Remove(toRemove);
                    }
                }
            }
        }

        private void LoadServers()
        {
            using (var db = new LiteDatabase(_dbPath))
            {
                foreach (var item in db.GetCollection<Server>().FindAll())
                {
                    Servers.Add(item);
                }
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

        public string SelectedServer { get => _selectedServer; set => SetProperty(ref _selectedServer, value); }

        public SecureString Password { get; set; }
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        public DelegateCommand LoginCommand { get => loginCommand ??= new DelegateCommand(OnLogin); }

        DialogCloseListener IDialogAware.RequestClose => _requestClose;

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
            _ = LoginTask();
        }

        private async Task LoginTask()
        {
            await _dispatcher.InvokeAsync(() =>
            {
                IsBusy = true;
                Message = null;
            });

            try
            {
                var serverUrl = SelectedServer;
                var username = UserName;
                var password = ConvertToUnsecureString(Password);

                // Validate first
                if (string.IsNullOrWhiteSpace(serverUrl) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        Message = new DisplayMessage(
                            "Login Failed. Please provide server, username and password.",
                            DisplayMessage.MESSAGE_TYPE.Error);
                    });

                    return;
                }

                // Add server if it doesn't already exist
                var serverExists = await _dispatcher.InvokeAsync(
                    () => Servers.Any(s => s.Url == serverUrl));

                if (!serverExists)
                {
                    var newServer = new Server
                    {
                        Url = serverUrl
                    };

                    // LiteDB is synchronous, so don't keep it open across awaits
                    using (var db = new LiteDatabase(_dbPath))
                    {
                        db.GetCollection<Server>().Insert(newServer);
                    }

                    await _dispatcher.InvokeAsync(() =>
                    {
                        Servers.Add(newServer);
                    });
                }

                await _dispatcher.InvokeAsync(() =>
                {
                    Message = new DisplayMessage("Logging you in...");
                });

                var client = new RestClient(serverUrl);

                var request = new RestRequest("/api/token/")
                    .AddParameter("username", username)
                    .AddParameter("password", password);

                var authResponse =
                    await client.ExecutePostAsync<LoginResultBody>(request);

                if (!authResponse.IsSuccessStatusCode)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        Message = new DisplayMessage(
                            $"Login Failed. {authResponse.ErrorMessage ?? authResponse.Content}",
                            DisplayMessage.MESSAGE_TYPE.Error);
                    });

                    return;
                }

                if (authResponse.Data == null ||
                    string.IsNullOrWhiteSpace(authResponse.Data.AccessToken))
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        Message = new DisplayMessage(
                            "Login Failed. The server returned an invalid authentication response.",
                            DisplayMessage.MESSAGE_TYPE.Error);
                    });

                    return;
                }

                await _dispatcher.InvokeAsync(() =>
                {
                    Message = new DisplayMessage(
                        "Fetching user info from remote server...");
                });

                LoginResult = authResponse.Data;

                request = new RestRequest("/api/accounts/session")
                    .AddHeader(
                        "Authorization",
                        $"Bearer {authResponse.Data.AccessToken}");

                var sessionResponse =
                    await client.ExecuteGetAsync<SessionInfo>(request);

                if (!sessionResponse.IsSuccessStatusCode)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        Message = new DisplayMessage(
                            $"Login Failed. {sessionResponse.ErrorMessage ?? sessionResponse.Content}",
                            DisplayMessage.MESSAGE_TYPE.Error);
                    });

                    return;
                }

                if (sessionResponse.Data == null)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        Message = new DisplayMessage(
                            "Login Failed. The server returned invalid session information.",
                            DisplayMessage.MESSAGE_TYPE.Error);
                    });

                    return;
                }

                // Update session state
                _sessionVariables.ServerUrl = serverUrl;

                _mapper.Map(authResponse.Data, _sessionVariables);
                _mapper.Map(sessionResponse.Data, _sessionVariables);

                SetupEmailErrorLogging(
                    sessionResponse.Data.LoggingEmail,
                    sessionResponse.Data.LoggingPassword);

                await _dispatcher.InvokeAsync(() =>
                {
                    Message = new DisplayMessage(
                        "Setting up Communication Bus...");
                });

                await SetupMessageBus();

                _requestClose.Invoke(
                    new DialogResult(ButtonResult.OK));
            }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    Message = new DisplayMessage(
                        $"Login Failed. {ex.Message}",
                        DisplayMessage.MESSAGE_TYPE.Error);
                });
            }
            finally
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    IsBusy = false;
                });
            }
        }

        private async Task SetupMessageBus()
        {
            try
            {
                var messageBus = _container.Resolve<MessageBus>();

                await messageBus.ConnectAsync();

                Debug.WriteLine(
                    $"Message bus connected: {messageBus.IsConnected}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Message bus setup failed: {ex}");

                // Intentionally ignore.
                // Login/application startup can continue without RabbitMQ.
            }
        }
    }
}
