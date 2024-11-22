using AutoMapper;
using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class ArchivedShipmentsViewModel : PageBase, INavigationAware
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override string Title => "Archived Shipments";

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




        public ArchivedShipmentsViewModel(ApiClient apiClient, IMapper mapper, IDialogCoordinator dialogCoordinator, 
            SessionVariables sessionVariables)
        {
            _apiClient = apiClient;
            _mapper = mapper;
            _dialogCoordinator = dialogCoordinator;
            _sessionVariables = sessionVariables;
            ArchivedShipments = CollectionViewSource.GetDefaultView(_shipments);
        }
    }
}
