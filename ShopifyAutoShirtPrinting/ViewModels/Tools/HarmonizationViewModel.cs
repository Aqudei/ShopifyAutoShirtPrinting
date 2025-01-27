using AutoMapper;
using Common.Api;
using Common.Models.Harmonisation;
using ImTools;
using NLog;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Views.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels.Tools
{
    internal class HarmonizationViewModel : PageBase
    {
        internal class HarmonizationComparer : IEqualityComparer<Models.Harmonisation.HSN>
        {
            public bool Equals(Models.Harmonisation.HSN x, Models.Harmonisation.HSN y)
            {
                // Handle cases where x or y could be null
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;

                return x.Code == y.Code;
            }

            public int GetHashCode(Models.Harmonisation.HSN obj)
            {
                // Handle null case
                if (obj == null) return 0;

                return obj.Code != null ? obj.Code.GetHashCode() : 0;
            }
        }

        public ObservableCollection<Models.Harmonisation.HSN> HarmonizationItems { get; set; } = new();
        public override string Title => "Harmonization";

        private DelegateCommand _importCommand;

        public DelegateCommand ImportCommand
        {
            get { return _importCommand ??= new DelegateCommand(OnImport); }
        }

        private DelegateCommand _syncCommand;
        private readonly ApiClient _api;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private DelegateCommand _newItemCommand;

        public DelegateCommand NewItemCommand
        {
            get { return _newItemCommand ??= new DelegateCommand(OnNewItem); }
        }

        private void OnNewItem()
        {
            _dialogService.ShowDialog("HarmonizationDialog", r =>
            {

            });
        }

        private DelegateCommand _DeleteSelectedCommand;

        public DelegateCommand DeleteSelectedCommand
        {
            get { return _DeleteSelectedCommand ??= new DelegateCommand(OnDeleteSelected); }
        }

        private async void OnDeleteSelected()
        {
            try
            {
                var selected = HarmonizationItems.Where(s => s.IsSelected).ToArray();
                foreach (var item in selected)
                {
                    await _api.DeleteHsnAsync(_mapper.Map<Common.Models.Harmonisation.HSN>(item));
                }
            }
            catch (Exception)
            {

            }
        }

        public DelegateCommand SyncCommand
        {
            get { return _syncCommand ??= new DelegateCommand(OnSync); }
        }

        private DelegateCommand _RefreshCommand;

        public DelegateCommand RefreshCommand
        {
            get { return _RefreshCommand ??= new DelegateCommand(OnRefresh); }
        }

        private void OnRefresh()
        {
            Task.Run(FetchItems);
        }

        private async void OnSync()
        {
            await Task.Run(FetchItems);
            await Task.Run(PushItems);
        }

        private async void PushItems()
        {
            var batchSize = 100;

            // Group the rows into batches of 10
            var batches = HarmonizationItems
                .Select((hsn, index) => new { hsn = hsn, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.hsn));

            foreach (var batch in batches)
            {
                await _api.SaveBulkHSNs(batch.Select(b => new Common.Models.Harmonisation.HSN
                {
                    Code = b.Code,
                    Description = b.Description,
                    Id = b.Id,
                }));
            }
        }

        private async Task FetchItems()
        {
            var harmonizationItems = await _api.ListHarmonizationsAsync();
            await _dispatcher.InvokeAsync(() =>
            {
                HarmonizationItems.Clear();
                HarmonizationItems.AddRange(harmonizationItems.Select(h => _mapper.Map<Models.Harmonisation.HSN>(h)));
            });
        }

        static IEnumerable<Models.Harmonisation.HSN> ImportExcel(string filePath)
        {
            var headers = new List<string>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet

            // Read header row
            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                headers.Add(worksheet.Cells[1, col].Text);
            }

            // Read data rows
            for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
            {
                var item = new Dictionary<string, string>();

                for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                {
                    item[headers[col - 1]] = worksheet.Cells[row, col].Text;
                }

                yield return new Models.Harmonisation.HSN
                {
                    Code = item["Code"],
                    Description = item["Description"]
                };
            }
        }


        private void OnImport()
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {

            };

            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Excel Files (*.xlsx)", "*.xlsx"));

            var result = dialog.ShowDialog();
            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Cancel)
                return;

            foreach (var item in ImportExcel(dialog.FileName))
            {
                if (!HarmonizationItems.Contains(item, new HarmonizationComparer()))
                {
                    HarmonizationItems.Add(item);
                }
            }
        }

        public HarmonizationViewModel(ApiClient api, IMapper mapper, IDialogService dialogService)
        {
            _api = api;
            _mapper = mapper;
            _dialogService = dialogService;
        }
    }
}
