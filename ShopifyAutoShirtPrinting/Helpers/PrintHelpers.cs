using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

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
            using (var client = new System.Net.Http.HttpClient())
            {

                using var response = await client.GetAsync(source);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(destination);

                await stream.CopyToAsync(fileStream);
            }

            return destination;
        }

        public static void PrintPdf(string pdfPath, string printerName)
        {
            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
                return;

            Task.Run(() =>
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
            });
        }
    }
}
