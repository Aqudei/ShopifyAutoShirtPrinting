using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Helpers;
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
        private DelegateCommand<string> _changePageCommand;
        private string _selectedPage;
        public ObservableCollection<Shipment> ShippedItems { get; set; } = new();
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

        public ShipmentsViewModel(IDialogCoordinator dialogCoordinator, IEventAggregator eventAggregator, ApiClient apiClient,
            SessionVariables sessionVariables)
        {
            _dialogCoordinator = dialogCoordinator;
            _eventAggregator = eventAggregator;
            _apiClient = apiClient;
            _sessionVariables = sessionVariables;
            _dispatcher = Application.Current.Dispatcher;

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
                    { "HasLabel", "1" },
                    { "Manifested", "0" },
                };

                var shipments = await _apiClient.FetchShipmentsByAsync(0, searchParams);

                _dispatcher.Invoke(() =>
                {
                    TotalPending = shipments.Count();
                });

                _dispatcher.Invoke(ShippedItems.Clear);
                foreach (var shipment in shipments)
                {
                    _dispatcher.Invoke(() => ShippedItems.Add(shipment));
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

                var shipments = await _apiClient.FetchShipmentsByAsync(offset, _searchParameters);

                _dispatcher.Invoke(ShippedItems.Clear);
                foreach (var shipment in shipments)
                {
                    _dispatcher.Invoke(() => ShippedItems.Add(shipment));
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
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        private DelegateCommand _manifestPendingCommand;

        public DelegateCommand ManifestPendingCommand
        {
            get { return _manifestPendingCommand ??= new DelegateCommand(OnManifestShipments); }
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

        private DelegateCommand<Shipment> _viewLabelCommand;

        public DelegateCommand<Shipment> ViewLabelCommand
        {
            get { return _viewLabelCommand ??= new DelegateCommand<Shipment>(OnViewLabel); }
        }


        private async void DownloadLabel(Shipment shipment, string labelPath)
        {
            using var client = new WebClient();


            await client.DownloadFileTaskAsync(shipment.Label, labelPath);
            if (File.Exists(labelPath))
            {
                Process.Start(labelPath);
            }
        }
        private async void OnViewLabel(Shipment shipment)
        {

            ProgressDialogController progress = null;
            try
            {
                if (shipment.Label == null)
                {
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

        private DelegateCommand<Shipment> _viewManifestCommand;
        private int _totalPending = 0;

        public DelegateCommand<Shipment> ViewManifestCommand
        {
            get { return _viewManifestCommand ??= new DelegateCommand<Shipment>(OnViewManifest); }
        }


        private async void OnViewManifest(Shipment shipment)
        {

            ProgressDialogController progress = null;
            try
            {
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
                    await client.DownloadFileTaskAsync(new Uri(shipment.ShipmentOrder.OrderSummary), manifestPdfPath);
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
