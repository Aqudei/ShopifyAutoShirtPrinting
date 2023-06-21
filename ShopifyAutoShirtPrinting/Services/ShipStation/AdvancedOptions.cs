using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class AdvancedOptions
    {
        //parentId number  Read-Only: If an order has been split, it will return the Parent ID of the order with which it has been split. If the order has not been split, this field will return null. Read Only
        //billToParty string Identifies which party to bill. Possible values: "my_account", "my_other_account" (see note below), "recipient", "third_party". billTo values can only be used when creating/updating orders.

        //billToAccount   string Account number of billToParty.billTo values can only be used when creating/updating orders.
        //billToPostalCode    string Postal Code of billToParty.billTo values can only be used when creating/updating orders.
        //billToCountryCode   string Country Code of billToParty.billTo values can only be used when creating/updating orders.
        //billToMyOtherAccount    string When using my_other_account billToParty value, the shippingProviderId value associated with the desired account.Make a List Carriers call to obtain shippingProviderId values.

        public int WarehouseId { get; set; }
        public bool NonMachinable { get; set; }
        public bool SaturdayDelivery { get; set; }
        public bool ContainsAlcohol { get; set; }
        public int StoreId { get; set; }
        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string Source { get; set; }
        public bool MergedOrSplit { get; set; }
        public List<int> MergedIds { get; set; }
        public int ParentId { get; set; }
        public string BillToParty { get; set; }
        public string BillToAccount { get; set; }
        public string BillToPostalCode { get; set; }
        public string BillToCountryCode { get; set; }
        public string BillToMyOtherAccount { get; set; }

        public AdvancedOptions()
        {
            MergedIds = new List<int>();
        }

    }
}
