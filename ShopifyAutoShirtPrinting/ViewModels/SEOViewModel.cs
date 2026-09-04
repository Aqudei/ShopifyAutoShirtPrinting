using AutoMapper;
using Common.Api;
using Common.Models;
using Common.Models.Seo;
using MahApps.Metro.Controls.Dialogs;
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
    public class SEOViewModel : PageBase, INavigationAware
    {
        private ObservableCollection<Models.Seo.SEOPage> _seoPages = new();
        public ICollectionView SEOPages { get; set; }


        private readonly ApiClient _client;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IDialogCoordinator _dialogCoordinator;

        public SEOViewModel(ApiClient client, IMapper mapper, IDialogService dialogService, IDialogCoordinator dialogCoordinator)
        {

            SEOPages = CollectionViewSource.GetDefaultView(_seoPages);
            _client = client;
            _mapper = mapper;
            _dialogService = dialogService;
            _dialogCoordinator = dialogCoordinator;

            Title = "SEO";
        }


        public async Task FetchSEOPagesTask()
        {
            var controller = await _dialogCoordinator.ShowProgressAsync(
                this,
                "Loading",
                "Fetching SEO Pages...");
            
            controller.SetIndeterminate();

            try
            {
                var seoPagesTask = _client.ListSEOPagesAsync();
                var auditsTask = _client.ListSEOAuditAsync();

                await Task.WhenAll(seoPagesTask, auditsTask);

                var seoPages = await seoPagesTask;
                var audits = await auditsTask;

                var auditsByPageId = audits
                    .GroupBy(a => a.PageId)
                    .ToDictionary(g => g.Key, g => g.First());

                var mappedPages = seoPages
                    .Select(page => _mapper.Map<Models.Seo.SEOPage>(page))
                    .ToList();

                foreach (var page in mappedPages)
                {
                    if (auditsByPageId.TryGetValue(page.PageId, out var pageAudit))
                    {
                        _mapper.Map(pageAudit, page);
                    }
                }

                await _dispatcher.InvokeAsync(() =>
                {
                    _seoPages.Clear();
                    
                    _seoPages.AddRange(mappedPages);
                });
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
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
        private DelegateCommand<Models.Seo.SEOPage> _showDetailCommand;

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

        public DelegateCommand<Models.Seo.SEOPage> ShowDetailCommand => _showDetailCommand ??= new DelegateCommand<Models.Seo.SEOPage>(OnShowDetail);

        private void OnShowDetail(Models.Seo.SEOPage page)
        {
            var p = new DialogParameters { { "page", page } };
            _dialogService.ShowDialog("SEODetailDialog", p, r =>
            {

            });
        }

        private void OnRowDoubleClick(Models.Seo.SEOPage seoPage)
        {
            //OnShowScoreCard(seoPage);
        }
    }
}
