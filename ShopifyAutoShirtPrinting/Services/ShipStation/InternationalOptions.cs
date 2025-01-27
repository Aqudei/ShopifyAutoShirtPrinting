using System.Collections.Generic;

namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class InternationalOptions
    {
        //contents string Contents of international shipment.Available options are: "merchandise", "documents", "gift", "returned_goods", or "sample"
        //customsItems CustomsItem An array of customs items.

        //    NOTE: To supply customsItems in the CreateOrder call and have the values not be overwritten by ShipStation, you must set the International Settings > Customs Declarations to "Leave blank (Enter Manually)" in the UI: https://ss.shipstation.com/#/settings/international

        //Please see our ShipStation's International Settings KB article to learn how to make this change in the UI.
        //    nonDelivery string Non-Delivery option for international shipment.Available options are: "return_to_sender" or "treat_as_abandoned".

        //NOTE: If the shipment is created through the Orders/CreateLabelForOrder endpoint and the nonDelivery field is not specified then value defaults based on the International Setting in the UI. If the call is being made to the Shipments/CreateLabel endpoint and the nonDelivery field is not specified then the value will default to "return_to_sender".

        public string Contents { get; set; }
        public List<CustomsItem> CustomsItems { get; set; }
        public string NonDelivery { get; set; }
    }
}
