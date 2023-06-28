using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public class TagUpdated
    {
        public int MyLineItemDatabaseId { get; set; }
    }

    public class StatusUpdated
    {
        public int MyLineItemDatabaseId { get; set; }
    }
}
