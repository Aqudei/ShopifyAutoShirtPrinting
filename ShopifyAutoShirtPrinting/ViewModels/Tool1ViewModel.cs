using Common.Api;
using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using ShopifyEasyShirtPrinting.Services;
using ShopifySharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class Tool1ViewModel : PageBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
        public string GCRFile { get => _gcrFile; set => SetProperty(ref _gcrFile, value); }

        public ObservableCollection<Variant> VariantsSearchResult { get; set; } = new();
        public ObservableCollection<Variant> VariantsQueue { get; set; } = new();

        private string _printFilesFolder;

        public string PrintFilesFolder
        {
            get => _printFilesFolder;
            set => SetProperty(ref _printFilesFolder, value);
        }

        private DelegateCommand _applySearch;

        private string _gcrFile;
        private string _searchText;
        private readonly ApiClient _apiClient;
        private readonly IDialogCoordinator _dialogCoordinator;

        public DelegateCommand ApplySearchCommand => _applySearch ??= new DelegateCommand(ApplySearch);
        private DelegateCommand _browseGcrFileCommand;

        public DelegateCommand BrowseGcrFileCommand => _browseGcrFileCommand ??= new DelegateCommand(HandleBrowseGcr);

        private void HandleBrowseGcr()
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {

            };

            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("GCR Files", "*.GCR;*.gcr"));
            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("PNG Files", "*.PNG;*.png"));
            var dlgResult = dialog.ShowDialog();

            if (dlgResult != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return;
            }

            GCRFile = dialog.FileName;
        }

        public override string Title => "Some Tools";

        private async void ApplySearch()
        {
            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", $"Searching for product similar to '{SearchText}'...");
            try
            {
                var result = await _apiClient.ListVariantsAsync(SearchText);
                if (result != null && result.Length > 0)
                {
                    await _dispatcher.InvokeAsync(VariantsSearchResult.Clear);
                    await _dispatcher.InvokeAsync(() => VariantsSearchResult.AddRange(result));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
            }
            finally
            {
                await progress.CloseAsync();
            }
        }

        private DelegateCommand<IList> _addToQueueCommand;
        private DelegateCommand addAllVariantsToQueue;

        public DelegateCommand<IList> AddToQueueCommand => _addToQueueCommand ??= new DelegateCommand<IList>(HandleAddVariantToQueue);

        private void HandleAddVariantToQueue(IList variants)
        {

            for (int i = variants.Count - 1; i >= 0; i--)
            {
                var variant = variants[i] as Variant;
                if (variant == null)
                    continue;

                if (VariantsQueue.Contains(variant))
                    continue;

                VariantsQueue.Add(variant);
            }
        }

        private DelegateCommand<IList> _removeFromQueueCommand;

        public DelegateCommand<IList> RemoveFromQueueCommand
        {
            get { return _removeFromQueueCommand ??= new DelegateCommand<IList>(HandleRemoveFromQueue); }
        }

        private void HandleRemoveFromQueue(IList variants)
        {
            for (int i = variants.Count - 1; i >= 0; i--)
            {
                var variant = variants[i] as Variant;
                if (variant != null)
                    VariantsQueue.Remove(variant);
            }
        }

        public DelegateCommand AddAllVariantsToQueueCommand => addAllVariantsToQueue ??= new DelegateCommand(HandleAddAllVariantToQueue);

        private void HandleAddAllVariantToQueue()
        {
            foreach (var variant in VariantsSearchResult)
            {
                if (VariantsQueue.Contains(variant))
                    continue;

                VariantsQueue.Add(variant);
            }
        }

        private DelegateCommand _removeAllFromQueueCommand;

        public DelegateCommand RemoveAllFromQueueCommand => _removeAllFromQueueCommand ??= new DelegateCommand(HandleRemoveAllFromQueue);

        private void HandleRemoveAllFromQueue()
        {
            VariantsQueue.Clear();
        }

        private DelegateCommand _duplicateGcrCommand;

        public DelegateCommand DuplicateGcrCommand => _duplicateGcrCommand ??= new DelegateCommand(HandleDuplicateGcr, CanDuplicateGcr)
            .ObservesProperty(() => GCRFile)
            .ObservesProperty(() => PrintFilesFolder);

        private bool CanDuplicateGcr()
        {
            return !string.IsNullOrWhiteSpace(GCRFile) && !string.IsNullOrWhiteSpace(PrintFilesFolder);
        }

        private async void HandleDuplicateGcr()
        {
            var erringFiles = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(PrintFilesFolder))
                return;

            var progress = await _dialogCoordinator.ShowProgressAsync(this, "Please wait", "Duplicating GCR/PNG File...");

            var ext = Path.GetExtension(GCRFile);

            foreach (var variant in VariantsQueue)
            {
                if (string.IsNullOrWhiteSpace(variant.Sku))
                    continue;

                var destination = Path.ChangeExtension(Path.Combine(PrintFilesFolder, variant.Sku), ext);
                try
                {
                    File.Copy(GCRFile, destination, true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message);

                    erringFiles.Add(destination, ex.Message);
                }
            }

            await progress.CloseAsync();

            if (erringFiles.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var errFile in erringFiles)
                {
                    sb.AppendLine($"{errFile.Key} - {errFile.Value}");
                }


                await _dialogCoordinator.ShowMessageAsync(this, "Attention1", $"Some files were not able to be generated.\n\n{sb}");
            }

        }

        private DelegateCommand _openPrintFilesFolderLocationCommand;

        public DelegateCommand OpenPrintFilesFolderLocationCommand => _openPrintFilesFolderLocationCommand ??= new DelegateCommand(HandleOpenPrintFilesLoc);

        private void HandleOpenPrintFilesLoc()
        {
            Process.Start("explorer", $"\"{PrintFilesFolder}\"");
        }

        public Tool1ViewModel(ApiClient apiClient, IDialogCoordinator dialogCoordinator)
        {
            _apiClient = apiClient;
            _dialogCoordinator = dialogCoordinator;


            PrintFilesFolder = Properties.Settings.Default.PrintFilesFolder;
        }
    }
}
