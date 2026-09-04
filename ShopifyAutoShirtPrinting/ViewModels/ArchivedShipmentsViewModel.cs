using AutoMapper;
using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Navigation.Regions;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Windows.Web.Http;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class ArchivedShipmentsViewModel : PageBase, INavigationAware
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        private ObservableCollection<Models.Shipment> _shipments = new();

        private readonly ApiClient _apiClient;
        private readonly IMapper _mapper;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly SessionVariables _sessionVariables;

        public ICollectionView ArchivedShipments { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _shipments.Clear();

            Task.Run(FetchArchivedShipments);
        }

        private async void FetchArchivedShipments()
        {
            var findShipments = new Dictionary<string, string> { { "HasLabel", "true" }, { "Manifested", "true" } };
            var shipments = await _apiClient.FetchShipmentsByAsync(getParameters: findShipments);

            foreach (var shipment in shipments)
            {
                await _dispatcher.BeginInvoke(() => _shipments.Add(_mapper.Map<Models.Shipment>(shipment)));
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
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = labelPath,
                        UseShellExecute = true
                    });
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
            using var client = new System.Net.Http.HttpClient();
            using var response = await client.GetAsync(shipment.Label);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(labelPath);
            await stream.CopyToAsync(fileStream);

            if (File.Exists(labelPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = labelPath,
                    UseShellExecute = true
                });
            }
        }

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
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = manifestPdfPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    progress = await _dialogCoordinator.ShowProgressAsync(this, "Opening", $"Opening order summary...");
                    progress.SetIndeterminate();
                    using var client = new System.Net.Http.HttpClient();
                    using var response = await client.GetAsync(new Uri(orderSummary));
                    response.EnsureSuccessStatusCode();

                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream = File.Create(manifestPdfPath);
                    await stream.CopyToAsync(fileStream);

                    if (File.Exists(manifestPdfPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = manifestPdfPath,
                            UseShellExecute = true
                        });
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




        public ArchivedShipmentsViewModel(ApiClient apiClient, IMapper mapper, IDialogCoordinator dialogCoordinator,
            SessionVariables sessionVariables)
        {
            Title = "Archived Shipments";
            _apiClient = apiClient;
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
            _sessionVariables = sessionVariables;
            ArchivedShipments = CollectionViewSource.GetDefaultView(_shipments);
        }
    }
}
