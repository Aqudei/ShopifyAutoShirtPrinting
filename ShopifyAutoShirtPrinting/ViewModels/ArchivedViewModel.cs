using Prism.Regions;
using ShopifyEasyShirtPrinting.Models;
using ShopifyEasyShirtPrinting.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class ArchivedViewModel : PageBase, INavigationAware
    {
        private readonly ApiClient _apiClient;

        public override string Title => "Archived";
        private ObservableCollection<MyLineItem> _items = new ObservableCollection<MyLineItem>();
        private ICollectionView items;
        private MyLineItem selectedLineItem;

        public ICollectionView LineItemsView { get => items; set => SetProperty(ref items, value); }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public ArchivedViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            LineItemsView = CollectionViewSource.GetDefaultView(_items);
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await LoadArchivedAsync();
        }


        public MyLineItem SelectedLineItem { get => selectedLineItem; set => SetProperty(ref selectedLineItem, value); }
        private async Task LoadArchivedAsync()
        {
            await _dispatcher.InvokeAsync(() => _items.Clear());

            var items = await _apiClient.ListArchivedItemsAsync();
            foreach (var item in items)
            {
                await _dispatcher.InvokeAsync(() => _items.Add(item));
            }
        }
    }
}
