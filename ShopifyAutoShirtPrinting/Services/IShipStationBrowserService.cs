using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    public interface IShipStationBrowserService
    {
        bool LoginCompleted { get; }

        void Dispose();
        void DoLogin();
        Task<bool> NavigateToOrderAsync(string orderNumber);
    }
}