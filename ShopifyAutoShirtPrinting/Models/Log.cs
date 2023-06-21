using ShopifyEasyShirtPrinting.Data;
using System;

namespace ShopifyEasyShirtPrinting.Models
{
    public class Log : EntityBase
    {
        public DateTime ChangeDate { get; set; }
        public string ChangeStatus { get; set; }

        public int MyLineItemId { get; set; }
        public virtual MyLineItem MyLineItem { get; set; }
    }
}
