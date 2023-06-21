using System;

namespace ShopifyEasyShirtPrinting.Services
{
    internal class DummyShipstatiionBrowserService : IDisposable, IShipStationBrowserService
    {
        public bool LoginSuccess => true;

        public void Dispose()
        {
        }

        public void DoLogin()
        {

        }

        public void NavigateToOrder(string orderNumber)
        {

        }
    }
}
