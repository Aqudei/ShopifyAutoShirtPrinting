using OpenQA.Selenium;
using Prism.Services.Dialogs;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Extensions
{
    public static class RestSharpExtentions
    {
        public static RestRequest AttachCookies(this RestRequest restRequest, IEnumerable<Cookie> cookies)
        {
            if (restRequest.CookieContainer == null)
            {
                restRequest.CookieContainer = new System.Net.CookieContainer();
            }

            foreach (var cookie in cookies)
            {
                if (cookie.Domain == null)
                {
                    continue;
                }

                restRequest.CookieContainer.Add(new System.Net.Cookie(cookie.Name, cookie.Value)
                {
                    Domain = cookie.Domain
                });
            }

            return restRequest;
        }
    }
}
