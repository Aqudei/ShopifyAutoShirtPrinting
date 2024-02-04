using AutoMapper;
using Common.Api;
using Common.Models;
using ImTools;
using PrintAgent.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrintAgent.ViewModels
{
    public class PrintingViewModel : PageBase
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ApiClient _apiClient;
        private readonly IRule _hotFolderRule;
        private readonly IMapper _mapper;
        private readonly BackgroundWorker _bgWorker;
        public ObservableCollection<Models.PrintRequest> PrintRequests { get; set; } = new();
        public override string Title => "Printing";

        public PrintingViewModel(ApiClient apiClient, IRule hotFolderRule, IMapper mapper)
        {
            _mapper = mapper;
            _apiClient = apiClient;
            _hotFolderRule = hotFolderRule;
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += _bgWorker_DoWork;
            _bgWorker.RunWorkerAsync();
        }


        private DelegateCommand _clearItemsCommand;

        public DelegateCommand ClearItemsCommand
        {
            get { return _clearItemsCommand ??= new DelegateCommand(OnClearItems); }
        }

        private void OnClearItems()
        {
            PrintRequests.Clear();
        }
        private void TryOpenPrintFiles(string printFile)
        {
            if (string.IsNullOrWhiteSpace(printFile) || !File.Exists(printFile))
            {
                return;
            }

            Process.Start(printFile);
        }

        private DelegateCommand<Models.PrintRequest> _openGcrCommand;

        public DelegateCommand<Models.PrintRequest> OpenGcrCommand
        {
            get { return _openGcrCommand ??= new DelegateCommand<Models.PrintRequest>(OnOpenGcr); }
        }

        private void OnOpenGcr(Models.PrintRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PrintFile) || !File.Exists(request.PrintFile))
            {
                return;
            }

            TryOpenPrintFiles(request.PrintFile);
        }

        private async void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    var printItems = await _apiClient.FetchPrintRequests();
                    if (printItems != null)
                    {
                        await _dispatcher.InvokeAsync(() =>
                        {
                            PrintRequests.AddRange(printItems.Select(p => _mapper.Map<Models.PrintRequest>(p)));
                        });

                        foreach (var printItem in PrintRequests)
                        {
                            if (string.IsNullOrWhiteSpace(printItem.LineItem.Sku))
                            {
                                await _dispatcher.InvokeAsync(() => printItem.Status = "Error - Item has no valid SKU.");
                                continue;
                            }

                            var sourcePng = Path.Combine(Properties.Settings.Default.PrintFilesFolder, printItem.LineItem.Sku + ".png");

                            if (File.Exists(sourcePng))
                            {
                                TryOpenPrintFiles(sourcePng);
                                await _dispatcher.InvokeAsync(() => printItem.Status = "Success");
                                await _dispatcher.InvokeAsync(() => printItem.PrintFile = sourcePng);
                            }
                            else
                            {
                                var sourceGcr = Path.Combine(Properties.Settings.Default.PrintFilesFolder, printItem.LineItem.Sku + ".gcr");
                                if (File.Exists(sourceGcr))
                                {
                                    TryOpenPrintFiles(sourceGcr);
                                    await _dispatcher.InvokeAsync(() => printItem.Status = "Success");
                                    await _dispatcher.InvokeAsync(() => printItem.Status = sourcePng);
                                }
                                else
                                {
                                    await _dispatcher.InvokeAsync(() => printItem.Status = "Error - No valid print file found!");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "@_bgWorker_DoWork()");
                }
            }
        }
    }
}
