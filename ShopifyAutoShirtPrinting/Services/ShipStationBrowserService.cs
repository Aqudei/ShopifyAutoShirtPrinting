using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace ShopifyEasyShirtPrinting.Services
{
    public class ShipStationBrowserService : IDisposable, IShipStationBrowserService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IWebDriver _driver;
        private readonly string _username;
        private readonly string _password;
        const string LoginUrl = "https://ss.shipstation.com/";

        public bool LoginSuccess { get; private set; }

        public ShipStationBrowserService()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());

            _driver = new ChromeDriver();

            _driver.Manage().Window.Maximize();
            _username = Environment.GetEnvironmentVariable("SHIPSTATION_USERNAME", EnvironmentVariableTarget.User);
            _password = Environment.GetEnvironmentVariable("SHIPSTATION_PASSWORD", EnvironmentVariableTarget.User);
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
                LoginSuccess = true;
            }
            catch (Exception)
            {
                LoginSuccess = false;
            }

        }

        public void NavigateToOrder(string orderNumber)
        {

            try
            {
                _driver.Navigate().GoToUrl("https://ship12.shipstation.com/orders/awaiting-shipment");

                var element = new WebDriverWait(_driver, TimeSpan.FromMinutes(120)).Until(
                    ExpectedConditions.ElementExists(By.Name("searchTerm")));

                element.SendKeys($"#{orderNumber}");
                Thread.Sleep(1000);
                element.SendKeys(Keys.Enter);

                Thread.Sleep(TimeSpan.FromSeconds(6));

                var selector =
                    "#app-root > div > div > div.main-content-29r3x8m > div.grid-content-24RrEJI > div.grid-and-footer-eAqjCp7 > div.grid-3F4ZAVB > div > div > div > div > div:nth-child(2) > div.non-pinned-columns-2EjEvkq > div > div > div:nth-child(1) > button";
                element = new WebDriverWait(_driver, TimeSpan.FromMinutes(120)).Until(
                    ExpectedConditions.ElementExists(By.CssSelector(selector)));
                element?.Click();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver?.Dispose();
        }
    }
}
