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
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

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
        const string LoginUrl = "https://ss.shipstation.com/";
        private const int TWO_MINUTES = 120;

        public bool LoginCompleted { get; private set; }

        public ShipStationBrowserService(SessionVariables sessionVariables)
        {
            new DriverManager().SetUpDriver(new ChromeConfig());

            _driver = new ChromeDriver();
            _apiClient = new SSApi2();

            _driver.Manage().Window.Maximize();
            _sessionVariables = sessionVariables;

            _username = _sessionVariables.ShipStationUsername;
            _password = _sessionVariables.ShipStationPassword;
        }


        public void DoLogin()
        {
            try
            {
                _driver.Navigate().GoToUrl(LoginUrl);
                var element =
                    new WebDriverWait(_driver, TimeSpan.FromMinutes(60)).Until(
                        ExpectedConditions.ElementExists(By.Id("username")));

                element.SendKeys(_username);
                element = new WebDriverWait(_driver, TimeSpan.FromMinutes(60)).Until(
                    ExpectedConditions.ElementExists(By.Id("password")));
                element.SendKeys(_password);

                element = new WebDriverWait(_driver, TimeSpan.FromMinutes(60)).Until(
                    ExpectedConditions.ElementExists(By.Id("btn-login")));
                element.Click();


                try
                {
                    element = new WebDriverWait(_driver, TimeSpan.FromMinutes(120)).Until(
                        ExpectedConditions.ElementExists(By.LinkText("Orders")));
                    LoginCompleted = true;

                    _apiClient.InitCookies(_driver);

                }
                catch (Exception)
                {
                    LoginCompleted = false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}\n\n{ex.StackTrace}");
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
