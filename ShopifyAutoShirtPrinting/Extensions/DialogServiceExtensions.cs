using Prism.Services.Dialogs;
using ShopifyEasyShirtPrinting.Services;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using ShopifySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Extensions
{
    public static class DialogServiceExtensions
    {
        public static void ShowChangePrintedQuantityDialog(this IDialogService dialogService)
        {

        }

        public static void ShowAfterScanDialog(this IDialogService dialogService, string title, string message, int id, Action<IDialogResult> callback)
        {
            var dlgParams = new DialogParameters
            {
                { "Title", title },
                { "Message", message },
                { "Id", id }
            };
            dialogService.ShowDialog("AfterScanDialog", dlgParams, callback);
        }

        public static void ShowLabelPrintingDialog(this IDialogService dialogService, string orderNumber, string message, Action<IDialogResult> callback)
        {
            var dlgParams = new DialogParameters
            {
                { "OrderNumber", orderNumber },
                { "Message", message }
            };

            dialogService.ShowDialog("LabelPrintingDialog", dlgParams, callback);
        }
    }
}
