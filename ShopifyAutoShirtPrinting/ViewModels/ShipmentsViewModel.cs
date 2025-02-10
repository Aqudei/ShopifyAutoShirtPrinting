using AutoMapper;
using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Messaging;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class ShipmentsViewModel : PageBase, INavigationAware
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IEventAggregator _eventAggregator;
        private readonly ApiClient _apiClient;
        private readonly SessionVariables _sessionVariables;
        private readonly IMapper _mapper;
        private readonly MessageBus _messageBus;
        private DelegateCommand<string> _changePageCommand;
        private string _selectedPage;
        public ObservableCollection<Models.Shipment> ShippedItems { get; set; } = new();
        public ObservableCollection<string> Pages { get; set; } = new();


        public string SelectedPage
        {
            get => _selectedPage;
            set => SetProperty(ref _selectedPage, value);
        }

        public override string Title => "Shipped";

        public DelegateCommand<string> GotoPageCommand => _changePageCommand ??= new DelegateCommand<string>(DoChangePage);

        private async void DoChangePage(string offset)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Fetching Shipments...");
            progress.SetIndeterminate();

            try
            {
                await Task.Run(() => FetchItems(int.Parse(offset)));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                await progress.CloseAsync();
            }
        }

        public ShipmentsViewModel(IDialogCoordinator dialogCoordinator, IEventAggregator eventAggregator,
            ApiClient apiClient, SessionVariables sessionVariables, IMapper mapper, MessageBus messageBus)
        {
            _dialogCoordinator = dialogCoordinator;
            _eventAggregator = eventAggregator;
            _apiClient = apiClient;
            _sessionVariables = sessionVariables;
            _mapper = mapper;
            _messageBus = messageBus;
            _dispatcher = Application.Current.Dispatcher;
        }

        private void _messageBus_ShipmentsUpdated(object sender, int[] shipmentIds)
        {
            Task.Run(async () =>
            {
                var shipments = await _apiClient.GetShipmentsByIdsAsync(shipmentIds);
                foreach (var shipment in shipments)
                {
                    var uiShipment = ShippedItems.FirstOrDefault(s => s.Id == shipment.Id);
                    if (uiShipment != null)
                    {
                        await _dispatcher.BeginInvoke(() => _mapper.Map(shipment, uiShipment));
                    }
                }
            });
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private DelegateCommand _searchCommand;
        private readonly Dictionary<string, string> _searchParameters = new();

        public DelegateCommand SearchCommand
        {
            get { return _searchCommand ??= new DelegateCommand(DoSearch); }
        }

        private async void DoSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _searchParameters.Clear();
                _searchParameters["offset"] = "0";
            }
            else
            {
                _searchParameters["OrderNumber"] = SearchText;
                _searchParameters["offset"] = "0";
            }

            await Task.Run(() => FetchItems());

        }

        public int TotalPending
        {
            get => _totalPending; set
            {
                SetProperty(ref _totalPending, value);
                RaisePropertyChanged(nameof(TotalPendingText));
            }
        }
        public string TotalPendingText => $"CREATE MANIFEST FOR ({TotalPending}) PENDING SHIPMENT/S";
        private async void FetchPendingShipments()
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Fetching Pending Shipments...");
            progress.SetIndeterminate();
            try
            {
                var searchParams = new Dictionary<string, string> {
                    { "HasLabel", "true" },
                    { "Manifested", "false" },
                };

                var shipments = await _apiClient.FetchShipmentsByAsync(getParameters: searchParams);

                _dispatcher.Invoke(() =>
                {
                    TotalPending = shipments.Count();
                });

                _dispatcher.Invoke(ShippedItems.Clear);
                foreach (var shipment in shipments)
                {
                    _dispatcher.Invoke(() => ShippedItems.Add(_mapper.Map<Models.Shipment>(shipment)));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                await progress.CloseAsync();
            }
        }

        private async void FetchItems(int offset = 0)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Fetching Shipments...");

            try
            {
                progress.SetIndeterminate();

                var shipments = await _apiClient.FetchShipmentsByAsync(offset, getParameters: _searchParameters);

                _dispatcher.Invoke(ShippedItems.Clear);
                foreach (var shipment in shipments)
                {
                    _dispatcher.Invoke(() => ShippedItems.Add(_mapper.Map<Models.Shipment>(shipment)));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                await progress.CloseAsync();
            }
        }

        public void Loaded()
        {
            //ShippedItems.Clear();
            //Task.Run(FetchShippedItems);
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await Task.Run(FetchPendingShipments);

            _messageBus.ShipmentsUpdated += _messageBus_ShipmentsUpdated;
            _messageBus.ShipmentsVoided += _messageBus_ShipmentsVoided;
        }

        private void _messageBus_ShipmentsVoided(object sender, int[] shipmentsIds)
        {
            Task.Run(async () =>
            {
                foreach (var shipmentId in shipmentsIds)
                {
                    var uiShipment = ShippedItems.FirstOrDefault(s => s.Id == shipmentId);
                    if (uiShipment != null)
                    {
                        await _dispatcher.BeginInvoke(() => ShippedItems.Remove(uiShipment));
                    }
                }
            });
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _messageBus.ShipmentsUpdated -= _messageBus_ShipmentsUpdated;
            _messageBus.ShipmentsVoided -= _messageBus_ShipmentsVoided;

        }

        private DelegateCommand _manifestPendingCommand;

        public DelegateCommand ManifestPendingCommand
        {
            get { return _manifestPendingCommand ??= new DelegateCommand(OnManifestShipments); }
        }

        private DelegateCommand _refreshCommand;

        public DelegateCommand RefreshCommand
        {
            get { return _refreshCommand ??= new DelegateCommand(DoRefresh); }
        }

        private async void DoRefresh()
        {
            await Task.Run(FetchPendingShipments);
        }

        private async void OnManifestShipments()
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading",
                $"Finalizing shipping orders. Generating manifest...");
            progress.SetIndeterminate();
            progress.SetCancelable(true);

            try
            {
                var shipmentOrder = await _apiClient.CreateManifestAsync();
                if (shipmentOrder != null)
                {
                    var timeStart = DateTime.Now;
                    var delta = DateTime.Now - timeStart;

                    while (delta <= TimeSpan.FromSeconds(120) && !progress.IsCanceled)
                    {
                        var orderSummary = shipmentOrder?.OrderSummary;

                        if (!string.IsNullOrWhiteSpace(orderSummary))
                        {
                            var labelPath = Path.Combine(_sessionVariables.PdfsPath, shipmentOrder.ManifestFileName);
                            var destination = await PrintHelpers.DownloadRemoteFileToLocalAsync(orderSummary, labelPath);
                            if (!string.IsNullOrWhiteSpace(destination) && File.Exists(destination))
                            {
                                PrintHelpers.PrintPdf(destination, Properties.Settings.Default.ManifestPrinter);
                            }
                            break;
                        }
                        else
                        { 
                            delta = DateTime.Now - timeStart;
                            shipmentOrder = await _apiClient.GetShipmentOrderByIdAsync(shipmentOrder.Id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                await progress.CloseAsync();
            }
        }


        private DelegateCommand<Models.Shipment> _voidLabelCommand;

        public DelegateCommand<Models.Shipment> VoidLabelCommand
        {
            get { return _voidLabelCommand ??= new DelegateCommand<Models.Shipment>(OnVoidLabel); }
        }

        private async void OnVoidLabel(Models.Shipment shipment)
        {
            ProgressDialogController progress = null;

            try
            {
                var confirm = await _dialogCoordinator.ShowMessageAsync(this, "Void Label",
                    $"Are you sure you want to void label of shipment for Order #{shipment.OrderNumber}?",
                    MessageDialogStyle.AffirmativeAndNegative);

                if (confirm == MessageDialogResult.Affirmative)
                {
                    progress = await _dialogCoordinator.ShowProgressAsync(this, "Void Label", "Voiding Labels... Please wait.");
                    progress.SetIndeterminate();

                    var updatedShipment = await _apiClient.VoidLabelForShipmentAsync(shipment.Id);
                    _mapper.Map(updatedShipment, shipment);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                if (progress != null)
                {
                    await progress.CloseAsync();
                }
            }
        }

        private DelegateCommand<Models.Shipment> _viewLabelCommand;

        public DelegateCommand<Models.Shipment> ViewLabelCommand
        {
            get
            {
                return _viewLabelCommand ??= new DelegateCommand<Models.Shipment>(OnViewLabel);
            }
        }
        private async void OnViewLabel(Models.Shipment shipment)
        {

            ProgressDialogController progress = null;
            try
            {
                if (shipment.Label == null || !shipment.HasLabel)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "View Label Error",
                        "This shipment has no label.");
                    return;
                }

                var nameOnly = Path.GetFileName(shipment.Label.ToString());
                var labelPath = Path.Combine(_sessionVariables.PdfsPath, nameOnly);
                if (File.Exists(labelPath))
                {
                    Process.Start(labelPath);
                }
                else
                {
                    progress = await _dialogCoordinator.ShowProgressAsync(this, "Label", "Downloading Label...");
                    progress.SetIndeterminate();

                    await Task.Run(() => DownloadLabel(shipment, labelPath));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                if (progress != null)
                    await progress.CloseAsync();
            }
        }


        private async void DownloadLabel(Models.Shipment shipment, string labelPath)
        {
            using var client = new WebClient();


            await client.DownloadFileTaskAsync(shipment.Label, labelPath);
            if (File.Exists(labelPath))
            {
                Process.Start(labelPath);
            }
        }


        private int _totalPending = 0;

        private DelegateCommand<Models.Shipment> _viewManifestCommand;

        public DelegateCommand<Models.Shipment> ViewManifestCommand
        {
            get { return _viewManifestCommand ??= new DelegateCommand<Models.Shipment>(OnViewManifest); }
        }


        private async void OnViewManifest(Models.Shipment shipment)
        {

            ProgressDialogController progress = null;
            try
            {
                var orderSummary = shipment?.ShipmentOrder?.OrderSummary;
                if (string.IsNullOrWhiteSpace(orderSummary))
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "View Manifest Error",
                        "This shipment has not been manifested yet.");
                    return;
                }

                var manifestPdfPath = Path.Combine(_sessionVariables.PdfsPath, shipment?.ShipmentOrder?.ManifestFileName);
                if (File.Exists(manifestPdfPath))
                {
                    Process.Start(manifestPdfPath);
                }
                else
                {
                    progress = await _dialogCoordinator.ShowProgressAsync(this, "Opening", $"Opening order summary...");
                    progress.SetIndeterminate();
                    using var client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(orderSummary), manifestPdfPath);
                    if (File.Exists(manifestPdfPath))
                    {
                        Process.Start(manifestPdfPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                if (progress != null)
                    await progress.CloseAsync();
            }
        }
    }
}
