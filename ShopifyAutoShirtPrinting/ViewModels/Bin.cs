using ShopifyEasyShirtPrinting.Models;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class Bin
    {
        public int BinNumber { get; set; }
        public string OrderNumber { get; set; }
        public MyLineItem[] LineItems { get; set; }
    }
}
