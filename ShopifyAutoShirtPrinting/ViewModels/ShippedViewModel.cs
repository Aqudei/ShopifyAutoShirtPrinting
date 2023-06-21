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
    internal class ShippedViewModel : PageBase, INavigationAware
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ShipStationApi _shipStationApi;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IEventAggregator _eventAggregator;

        private DelegateCommand<string> _changePageCommand;
        private string _selectedPage;
        public ObservableCollection<Order> ShippedItems { get; set; } = new();
        public ObservableCollection<string> Pages { get; set; } = new();


        public string SelectedPage
        {
            get => _selectedPage;
            set => SetProperty(ref _selectedPage, value);
        }

        public override string Title => "Shipped";

        public DelegateCommand<string> GotoPageCommand => _changePageCommand ??= new DelegateCommand<string>(DoChangePage);

        private async void DoChangePage(string pageNumber)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Fetching Shipments...");
            progress.SetIndeterminate();

            try
            {
                await Task.Run(() => FetchShippedItems(int.Parse(pageNumber)));
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

        public ShippedViewModel(ShipStationApi shipStationApi, IDialogCoordinator dialogCoordinator, IEventAggregator eventAggregator)
        {
            _shipStationApi = shipStationApi;
            _dialogCoordinator = dialogCoordinator;
            _eventAggregator = eventAggregator;
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
                await Task.Run(() => FetchShippedItems());
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

        private async void FetchShippedItems(int pageNum = 1)
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Loading",
                "Fetching shipped items from ShipStaion at Page #1");
            progress.SetIndeterminate();

            try
            {
                _searchParameters["page"] = $"{pageNum}";

                var listOrdersResponse = await _shipStationApi.ListShippedOrdersAsync(_searchParameters);

                _dispatcher.Invoke(Pages.Clear);
                _dispatcher.Invoke(ShippedItems.Clear);

                if (listOrdersResponse == null) return;

                for (var i = 0; i < listOrdersResponse.Pages; i++)
                {
                    var x = i;
                    _dispatcher.Invoke(() => { Pages.Add($"{x + 1}"); });
                }

                foreach (var order in listOrdersResponse.Orders)
                {
                    _dispatcher.Invoke(() => ShippedItems.Add(order));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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
            await Task.Run(() => FetchShippedItems());
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }
    }
}
