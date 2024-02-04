using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Messaging
{
    public class ItemsUpdated
    {
        public int[] MyLineItemDatabaseIds { get; set; }
    }
}
