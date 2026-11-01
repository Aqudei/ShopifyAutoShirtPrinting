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

        private DelegateCommand _selectAllCommand;

        public DelegateCommand SelectAllCommand
        {
            get { return _selectAllCommand ??= new DelegateCommand(OnSelectAllSEOPages); }
        }

        private void OnSelectAllSEOPages()
        {
            foreach (var seoPage in _seoPages)
            {
                seoPage.IsSelected = true;
            }
        }

        private DelegateCommand _ClearSelectionCommand;

        public DelegateCommand ClearSelectionCommand
        {
            get { return _ClearSelectionCommand ??= new DelegateCommand(OnClearSEOPageSelection); }
        }

        private void OnClearSEOPageSelection()
        {
            foreach (var seoPage in _seoPages)
            {
                seoPage.IsSelected = false;
            }
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

        private DelegateCommand _refreshCommand;

        public DelegateCommand RefreshCommand
        {
            get { return _refreshCommand ??= new DelegateCommand(OnRefreshItemsAsync); }
        }

        private async void OnRefreshItemsAsync()
        {
            await FetchSEOPagesTask();
        }

        private DelegateCommand _runAuditCommand;

        public DelegateCommand RunAuditCommand
        {
            get { return _runAuditCommand ??= new DelegateCommand(OnRunAudit); }
        }

        private async void OnRunAudit()
        {
            var selectedPages = _seoPages.Where(p => p.IsSelected).ToList();

            // 1. Guard clause: Prevent the dialog from showing if nothing is selected
            if (!selectedPages.Any())
            {
                await _dialogCoordinator.ShowMessageAsync(this, "No Selection", "Please select at least one page to audit.");
                return;
            }

            // 2. Fixed typo: 'progess' -> 'progress'
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Running SEO Audit", "Initializing...");

            // 3. UX Improvement: Use determinate progress since we know the total count
            progress.Maximum = selectedPages.Count;
            progress.SetProgress(0);

            try
            {
                for (int i = 0; i < selectedPages.Count; i++)
                {
                    var page = selectedPages[i];

                    // Update the dialog message to show exactly what is happening
                    progress.SetMessage($"Auditing: {page.PageTitle} ({i + 1} of {selectedPages.Count})");

                    await _client.RunAuditAsync(page.PageId);

                    // Increment the visual progress bar
                    progress.SetProgress(i + 1);
                }
            }
            catch (Exception ex)
            {
                // 4. Error Handling: Inform the user if the API call fails
                await _dialogCoordinator.ShowMessageAsync(this, "Audit Failed", $"An error occurred during the audit:\n\n{ex.Message}");
            }
            finally
            {
                // 5. Safety: Guarantee the dialog closes even if an exception is thrown
                if (progress.IsOpen)
                {
                    await progress.CloseAsync();
                }
            }
        }

        private DelegateCommand _generateDraftCommand;

        public DelegateCommand GenerateDraftCommand
        {
            get { return _generateDraftCommand ??= new DelegateCommand(OnGenerateDraft); }
        }

        private async void OnGenerateDraft()
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Generating Drafts", "Initializing...");

            try
            {
                var selectedPages = _seoPages.Where(p => p.IsSelected).ToArray();

                for (int i = 0; i < selectedPages.Length; i++)
                {
                    var page = selectedPages[i];
                    progress.SetProgress((double)(i + 1) / selectedPages.Length);
                    progress.SetMessage($"Generating draft for: {page.PageTitle} ({i + 1} of {selectedPages.Length})");
                    await _client.GenerateDraftAsync(page.PageId);
                }
            }
            catch (Exception ex)
            {
                // Close the progress overlay first so the error dialog isn't hidden behind it
                if (progress.IsOpen)
                {
                    await progress.CloseAsync();
                }

                await _dialogCoordinator.ShowMessageAsync(this, "Generation Failed", $"An error occurred while generating drafts:\n\n{ex.Message}");
            }
            finally
            {
                // Fallback to ensure the progress dialog is always closed on success
                if (progress.IsOpen)
                {
                    await progress.CloseAsync();
                }
            }
        }


        private DelegateCommand<Models.Seo.SEOPage> _showDraftCommand;

        public DelegateCommand<Models.Seo.SEOPage> ShowDraftCommand
        {
            get { return _showDraftCommand ??= new DelegateCommand<Models.Seo.SEOPage>(OnShowDraft); }
        }

        private async void OnShowDraft(Models.Seo.SEOPage page)
        {
            if (page == null)
            {
                return;
            }

            try
            {
                var draft = await _client.GetSEOPageDraftAsync(page.PageId);

                if (draft == null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Draft Not Found", $"No draft was found for the page: {page.PageTitle}");
                    return;
                }

                var parameters = new DialogParameters { { "draft", _mapper.Map<Models.Seo.SEODraft>(draft) } };

                _dialogService.ShowDialog("SEODetailDialog", parameters, result =>
                {
                    // Intentional empty callback; handle dialog result here if required in the future.
                });
            }
            catch (Exception ex)
            {
                // Note: It is highly recommended to log the exception (ex) here for diagnostic purposes.
                await _dialogCoordinator.ShowMessageAsync(this, "Error Fetching Draft", $"An unexpected error occurred while retrieving the draft for the page: {page.PageTitle}");
            }
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
