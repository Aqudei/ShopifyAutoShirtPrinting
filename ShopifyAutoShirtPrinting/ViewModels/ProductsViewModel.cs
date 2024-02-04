using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Regions;
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
    internal class ProductsViewModel : PageBase, INavigationAware
    {
        private readonly ApiClient _apiClient;
        private readonly IDialogCoordinator _dialogCoordinator;
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Variant> _variants = new();


        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }

        private ICollectionView products;
        private ICollectionView variants;
        private Product _selectedProduct;
        private string _nextUrl;
        private string _previousUrl;

        public ICollectionView Products { get => products; set => SetProperty(ref products, value); }
        public ICollectionView Variants { get => variants; set => SetProperty(ref variants, value); }
        public Product SelectedProduct { get => _selectedProduct; set => SetProperty(ref _selectedProduct, value); }
        public override string Title => "Products";

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await Task.Run(FetchProductsFirstPage);
        }

        private async Task FetchProductsFirstPage()
        {
            var reqParams = new Dictionary<string, string>
            {
                {"limit","250" },
                {"offset","0" }
            };

            var productsResult = await _apiClient.ListProductsAsync(reqParams);
            await _dispatcher.InvokeAsync(_products.Clear);
            foreach (var p in productsResult.Results)
            {
                await _dispatcher.InvokeAsync(() => _products.Add(p));
            }

            _nextUrl = productsResult.Next;
            _previousUrl = productsResult.Previous;
        }


        private DelegateCommand<string> _navCommand;

        public DelegateCommand<string> NavCommand
        {
            get { return _navCommand ??= new DelegateCommand<string>(OnNavCommand); }
        }
        private DelegateCommand _searchCommand;

        public DelegateCommand SearchCommand
        {
            get { return _searchCommand ??= new DelegateCommand(OnSearch); }
        }

        private async void OnSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {

            }
        }

        private async void OnNavCommand(string nav)
        {
            PaginatedResult<Product> result = null;

            if (nav.ToLower() == "next" && !string.IsNullOrWhiteSpace(_nextUrl))
            {
                result = await _apiClient.FetchPage<Product>(_nextUrl);
            }

            if (nav.ToLower() == "previous" && !string.IsNullOrWhiteSpace(_previousUrl))
            {
                result = await _apiClient.FetchPage<Product>(_previousUrl);
            }

            if (result != null)
            {
                await _dispatcher.InvokeAsync(_products.Clear);
                foreach (var p in result.Results)
                {
                    await _dispatcher.InvokeAsync(() => _products.Add(p));
                }

                _nextUrl = result.Next;
                _previousUrl = result.Previous;
            }
        }

        public ProductsViewModel(ApiClient apiClient, IDialogCoordinator dialogCoordinator)
        {
            this._apiClient = apiClient;
            this._dialogCoordinator = dialogCoordinator;
            Products = CollectionViewSource.GetDefaultView(_products);
            Variants = CollectionViewSource.GetDefaultView(_variants);

            PropertyChanged += ProductsViewModel_PropertyChanged;
        }

        private async void ProductsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case (nameof(SelectedProduct)):
                    {
                        if (SelectedProduct == null)
                        {
                            return;
                        }

                        var variants = await _apiClient.FetchProductVariantsAsync(SelectedProduct.Id);
                        await _dispatcher.InvokeAsync(_variants.Clear);
                        foreach (var variant in variants)
                        {
                            await _dispatcher.InvokeAsync(() =>
                            {
                                _variants.Add(variant);
                            });
                        }

                        return;
                    }
                case nameof(SearchText):
                    {
                        if (string.IsNullOrWhiteSpace(SearchText))
                        {
                            Products.Filter = null;
                            return;
                        }

                        Products.Filter = i =>
                        {
                            return (i as Product).Handle.ToLower().Contains(SearchText.ToLower());

                        };
                        return;
                    }
                default:
                    break;
            }

        }
    }
}
