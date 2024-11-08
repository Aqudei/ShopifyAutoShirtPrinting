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
    public class ManifestPrintTask : PrintTaskBase, IBGTask
    {
        private readonly ApiClient _apiClient;
        private readonly SessionVariables _sessionVariables;

        private ShipmentOrder _shipmentOrder;

        public ManifestPrintTask(ApiClient apiClient, SessionVariables sessionVariables, ShipmentOrder shipmentOrder)
        {
            _sessionVariables = sessionVariables;
            _apiClient = apiClient;
            _shipmentOrder = shipmentOrder;
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
                var orderSummary = _shipmentOrder?.OrderSummary;

                if (!string.IsNullOrWhiteSpace(orderSummary))
                {
                    var labelPath = Path.Combine(_sessionVariables.PdfsPath, _shipmentOrder.ManifestFileName);
                    var destination = await DownloadRemoteFileToLocalAsync(orderSummary, labelPath);
                    if (!string.IsNullOrWhiteSpace(destination) && File.Exists(destination))
                    {
                        PrintPdf(destination, ShopifyEasyShirtPrinting.Properties.Settings.Default.ManifestPrinter);
                    }

                    ContinueRunning = false;
                    break;
                }
                else
                {
                    _shipmentOrder = await _apiClient.GetShipmentOrderByIdAsync(_shipmentOrder.Id);
                }
            }
        }
    }
}
