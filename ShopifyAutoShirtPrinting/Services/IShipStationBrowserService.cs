namespace ShopifyEasyShirtPrinting.Services
{
    public interface IShipStationBrowserService
    {
        bool LoginSuccess { get; }

        void Dispose();
        void DoLogin();
        void NavigateToOrder(string orderNumber);
    }
}