using ShopifyEasyShirtPrinting.Models;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class Bin
    {
        public int BinNumber { get; set; }
        public List<MyLineItem> Items { get; set; } = new();
    }
}
