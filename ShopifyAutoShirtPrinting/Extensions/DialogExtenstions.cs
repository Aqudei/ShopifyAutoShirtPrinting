using Common.Models;
using MahApps.Metro.Controls.Dialogs;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Extensions
{
    public static class DialogExtenstions
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

        public static void ShowLabelPrintingDialog(this IDialogService dialogService, string orderNumber, int storeId, string message, Action<IDialogResult> callback)
        {
            var dlgParams = new DialogParameters
            {
                { "OrderNumber", orderNumber },
                { "Message", message },
                { "StoreId", storeId}
            };

            dialogService.ShowDialog("LabelPrintingDialog", dlgParams, callback);
        }

        public static async Task ShowExceptionErrorAsync(this IDialogCoordinator dialogCoordinator, object context, Exception exception)
        {
            await dialogCoordinator.ShowMessageAsync(context, "Caught Exception", $"{exception.Message}\n\n{exception.StackTrace}");
        }


        public static void ShowUpdateTagsConfirmationDialog(this IDialogService dialogService, string newTag, IEnumerable<LineItemViewModel> lineItems, Action<IDialogResult> callback)
        {
            var @params = new DialogParameters { { "Items", lineItems }, { "NewTag", newTag } };
            dialogService.ShowDialog("UpdateTagsDialog", @params, callback);
        }
    }
}
