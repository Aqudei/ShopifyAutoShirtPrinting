using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.BGTasker
{
    public abstract class PrintTaskBase
    {
        protected async Task<string> DownloadRemoteFileToLocalAsync(string source, string destination)
        {

            return await DownloadRemoteFileToLocalAsync(new Uri(source), destination);
        }

        protected async Task<string> DownloadRemoteFileToLocalAsync(Uri source, string destination)
        {
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(source, destination);
            }

            return destination;
        }
    }
}
