using AutoMapper;
using Common.Api;
using Common.Models;
using Common.Models.Seo;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Navigation.Regions;
using ShopifyEasyShirtPrinting.Models.Seo;
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
        private ObservableCollection<Models.Seo.SEOPage> _seoPages = new();
        public ICollectionView SEOPages { get; set; }

        public override string Title => "SEO";

        private readonly ApiClient _client;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;

        public SEOViewModel(ApiClient client, IMapper mapper, IDialogService dialogService)
        {

            SEOPages = CollectionViewSource.GetDefaultView(_seoPages);
            _client = client;
            this._mapper = mapper;
            this._dialogService = dialogService;
        }


        public async void FetchSEOPages()
        {
            var seoPages = await _client.ListSEOPagesAsync();
            var audits = await _client.ListSEOAuditAsync();

            await _dispatcher.InvokeAsync(() =>
            {
                _seoPages.Clear();
                var mappedPages = seoPages.Select(page => _mapper.Map<Models.Seo.SEOPage>(page)).ToList();
                foreach (var page in mappedPages)
                {
                    var pageAudit = audits.FirstOrDefault(audit => audit.PageId == page.PageId);

                    if (pageAudit != null)
                        _mapper.Map(pageAudit, page);
                }

                _seoPages.AddRange(mappedPages);
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

        private DelegateCommand<Models.Seo.SEOPage> _rowDoubleClickCommand;

        public DelegateCommand<Models.Seo.SEOPage> RowDoubleClickCommand
        {
            get { return _rowDoubleClickCommand ??= new DelegateCommand<Models.Seo.SEOPage>(OnRowDoubleClick); }
        }

        private DelegateCommand<Models.Seo.SEOPage> _showScoreCardCommand;

        public DelegateCommand<Models.Seo.SEOPage> ShowScoreCardCommand
        {
            get { return _showScoreCardCommand ??= new DelegateCommand<Models.Seo.SEOPage>(OnShowScoreCard); }
        }

        private void OnShowScoreCard(Models.Seo.SEOPage page)
        {
            var p = new DialogParameters { { "breakdown", page.Breakdown } };
            _dialogService.ShowDialog("ScoreBreakdownView", p, r =>
            {

            });
        }

        private void OnRowDoubleClick(Models.Seo.SEOPage seoPage)
        {
            OnShowScoreCard(seoPage);
        }
    }
}
