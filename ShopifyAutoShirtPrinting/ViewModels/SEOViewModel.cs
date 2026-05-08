using Common.Api;
using Common.Models;
using Common.Models.Seo;
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
    internal class SEOViewModel : PageBase, INavigationAware
    {
        private ObservableCollection<SEOPage> _seoPages = new();
        public ICollectionView SEOPages { get; set; }

        public override string Title => "SEO";

        private readonly ApiClient _client;

        public SEOViewModel(ApiClient client)
        {
            SEOPages = CollectionViewSource.GetDefaultView(_seoPages);
            _client = client;
        }


        public async void FetchSEOPages()
        {
            var seoPages = await _client.ListSEOPagesAsync();
            await _dispatcher.InvokeAsync(() =>
            {
                _seoPages.Clear();
                _seoPages.AddRange(seoPages);
            });
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Task.Run(FetchSEOPages);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private DelegateCommand<SEOPage>  _rowDoubleClickCommand;

        public DelegateCommand<SEOPage> RowDoubleClickCommand
        {
            get { return _rowDoubleClickCommand ??= new DelegateCommand<SEOPage>(OnRowDoubleClick); }
        }

        private void OnRowDoubleClick(SEOPage seoPage)
        {

        }
    }
}
