using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Models
{
    public static class OrderStatusDisplay
    {
        public enum OrderStatus
        {
            Pending,
            Processed,
            LabelPrinted,
            Shipped,
            Archived,
            IssueNeedsResolving,
            NeedToOrderFromSupplier,
            HaveOrderedFromSupplier
        }

        public static readonly Dictionary<OrderStatus, string> OrderStatusNames = new()
        {
            { OrderStatus.Pending, "Pending" },
            { OrderStatus.Processed, "Processed" },
            { OrderStatus.LabelPrinted, "Label Printed" },
            { OrderStatus.Shipped, "Shipped" },
            { OrderStatus.Archived, "Archived" },
            { OrderStatus.IssueNeedsResolving, "Issue Needs Resolving" },
            { OrderStatus.NeedToOrderFromSupplier, "Need To Order From Supplier" },
            { OrderStatus.HaveOrderedFromSupplier, "Have Ordered From Supplier" }
        };
    }
}
