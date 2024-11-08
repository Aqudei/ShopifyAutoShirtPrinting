using Common.Api;
using Common.BGTasker;
using Common.Models;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.BGTasker
{
    public class LabelPrintTask : PrintTaskBase, IBGTask
    {
        private readonly ApiClient _apiClient;
        private readonly SessionVariables _sessionVariables;
        private readonly Shipment _shipment;

        public LabelPrintTask(ApiClient apiClient, SessionVariables sessionVariables, Shipment shipment)
        {
            _sessionVariables = sessionVariables;
            _apiClient = apiClient;
            _shipment = shipment;
        }

        public static void PrintPdf(string pdfPath, string printerName = null)
        {
            // Open the PDF document
            using (var document = PdfDocument.Load(pdfPath))
            {
                // Create a PrintDocument object
                using (var printDocument = document.CreatePrintDocument())
                {
                    // Set the printer name, if specified
                    if (!string.IsNullOrEmpty(printerName))
                    {
                        printDocument.PrinterSettings.PrinterName = printerName;
                    }

                    // Print the document
                    printDocument.Print();
                }
            }
        }
        public override async void Execute()
        {
            while (ContinueRunning)
            {
                var shipment = await _apiClient.GetShipmentByAsync(new Dictionary<string, string> { { "OrderNumber", _shipment.OrderNumber } });
                if (shipment == null)
                    break;


                if (shipment.HasLabel && shipment.Label != null)
                {
                    var nameOnly = Path.GetFileName(shipment.Label.ToString());
                    var labelPath = Path.Combine(_sessionVariables.PdfsPath, nameOnly);
                    var destination = await DownloadRemoteFileToLocalAsync(shipment.Label, labelPath);
                    if (!string.IsNullOrWhiteSpace(destination) && File.Exists(destination))
                    {
                        PrintPdf(destination, ShopifyEasyShirtPrinting.Properties.Settings.Default.LabelPrinter);
                    }

                    ContinueRunning = false;
                    break;
                }
                else
                    await Task.Delay(4000);
            }

            Status = "";
        }
    }
}
