using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class ShipmentsViewModel : PageBase, INavigationAware
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IEventAggregator _eventAggregator;
        private readonly ApiClient _apiClient;
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

        public ShipmentsViewModel(IDialogCoordinator dialogCoordinator, IEventAggregator eventAggregator, ApiClient apiClient)
        {
            _dialogCoordinator = dialogCoordinator;
            _eventAggregator = eventAggregator;
            _apiClient = apiClient;
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
                _searchParameters["page"] = "1";
            }
            else
            {
                _searchParameters["orderNumber"] = SearchText;
                _searchParameters["page"] = "1";
            }

            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Fetching Shipments...");
            progress.SetIndeterminate();

            try
            {
                await Task.Run(() => FetchItems());
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

        private async void FetchItems(int offset = 0)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading",
                $"Fetching shipped items from ShipStation at Page #{offset+1}");
            progress.SetIndeterminate();


            var shipments = await _apiClient.FetchShipmentsAsync(offset);

            _dispatcher.Invoke(ShippedItems.Clear);
            foreach (var shipment in shipments)
            {
                _dispatcher.Invoke(()=>ShippedItems.Add(shipment));
            }

            await progress.CloseAsync();
        }

        public void Loaded()
        {
            //ShippedItems.Clear();
            //Task.Run(FetchShippedItems);
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await Task.Run(() => FetchItems());
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }
    }
}
