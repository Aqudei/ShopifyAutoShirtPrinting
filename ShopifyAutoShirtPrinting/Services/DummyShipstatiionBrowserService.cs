using System;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    internal class DummyShipstatiionBrowserService : IDisposable, IShipStationBrowserService
    {
        public bool LoginCompleted => true;

        public void Dispose()
        {
        }

        public void DoLogin()
        {

        }

        public Task<bool> NavigateToOrderAsync(string orderNumber)
        {
            return Task.Run(() => true);
        }
    }
}
