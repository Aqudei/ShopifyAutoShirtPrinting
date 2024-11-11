using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Helpers
{
    internal static class PrintHelpers
    {
        public static async Task<string> DownloadRemoteFileToLocalAsync(string source, string destination)
        {

            return await DownloadRemoteFileToLocalAsync(new Uri(source), destination);
        }

        public static async Task<string> DownloadRemoteFileToLocalAsync(Uri source, string destination)
        {
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(source, destination);
            }

            return destination;
        }

        public static void PrintPdf(string pdfPath, string printerName)
        {
            // Open the PDF document
            using var document = PdfDocument.Load(pdfPath);
            // Create a PrintDocument object
            using var printDocument = document.CreatePrintDocument();
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
