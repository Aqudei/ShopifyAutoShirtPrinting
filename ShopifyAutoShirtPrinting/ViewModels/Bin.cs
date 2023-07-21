using ShopifyEasyShirtPrinting.Models;
using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    public class Bin
    {
        public long Id { get; set; }
        public int BinNumber { get; set; }
        public string OrderNumber { get; set; }
        public MyLineItem[] LineItems { get; set; }

        public string Notes { get; set; }
        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    }
}
