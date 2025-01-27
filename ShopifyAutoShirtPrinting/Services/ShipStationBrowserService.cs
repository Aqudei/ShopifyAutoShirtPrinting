using Common.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using ShopifyEasyShirtPrinting.Helpers;
using ShopifyEasyShirtPrinting.Services.ShipStation;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Services
{
    public class ShipStationBrowserService : IDisposable, IShipStationBrowserService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IWebDriver _driver;
        private readonly SSApi2 _apiClient;
        private readonly string _username;
        private readonly string _password;
        private readonly SessionVariables _sessionVariables;
        const string LoginUrl = "https://ship.shipstation.com/";
        // const string LoginUrl = "https://ss.shipstation.com/";
        private const int TWO_MINUTES = 120;
        private const int _30_SECONDS = 30;

        public bool LoginCompleted { get; private set; }

        public ShipStationBrowserService(SessionVariables sessionVariables)
        {
            _sessionVariables = sessionVariables;

            _username = _sessionVariables.ShipStationUsername;
            _password = _sessionVariables.ShipStationPassword;

            // Path to the User Data directory where Chrome stores profiles
            var userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome for Testing\\User Data");
            // string userDataDir = @"C:\Users\<YourUsername>\AppData\Local\Google\Chrome\User Data";

            // Name of the specific profile you want to use (e.g., "Default" or "Profile 1")
            var profileName = "Default";
            // Setup Chrome options to use a specific profile
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument($"user-data-dir={userDataDir}");
            chromeOptions.AddArgument($"profile-directory={profileName}");
            chromeOptions.BinaryLocation = "C:\\chrome-win64\\chrome.exe";

            _driver = new ChromeDriver(chromeOptions);
            _apiClient = new SSApi2();

            _driver.Manage().Window.Maximize();
        }

        public void DoLogin()
        {
            try
            {
                // Check if already logged in via URL redirection
                _driver.Navigate().GoToUrl(LoginUrl);

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
                if (wait.Until(ExpectedConditions.UrlContains("ship12.shipstation.com/orders/awaiting-shipment")))
                {
                    _apiClient.InitCookies(_driver); // Initialize cookies after successful login
                    LoginCompleted = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"URL check Timeout: {ex.Message}\n\n{ex.StackTrace}");
                LoginCompleted = false;
            }
        }

        public async Task<bool> NavigateToOrderAsync(string orderNumber)
        {
            try
            {
                var result = await _apiClient.QuickSearchAsync(orderNumber);

                if (result == null || result.SalesOrders == null || !result.SalesOrders.Any())
                {
                    return false;
                }

                var salesOrder = result.SalesOrders.FirstOrDefault(x => x.OrderNumber == orderNumber);

                if (salesOrder == null)
                {
                    salesOrder = result.SalesOrders.First();
                }

                var fulfillmentPlanId = salesOrder.FulfillmentPlanIds.FirstOrDefault();

                var navUrl = $"https://ship12.shipstation.com/orders/all-orders-search-result/order/{salesOrder.SalesOrderId}/active/{fulfillmentPlanId}";
                _driver.Navigate().GoToUrl(navUrl);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver?.Dispose();
        }
    }
}
