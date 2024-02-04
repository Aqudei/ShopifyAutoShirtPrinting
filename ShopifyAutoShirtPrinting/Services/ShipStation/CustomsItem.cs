namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class CustomsItem
    {
        //customsItemId string Read Only field.When this field is not submitted in the Order/CreateOrder call, it will create a new customs line.If this field is included when submitting an order through Order/CreateOrder, then it will look for the corresponding customs line and update any values.
        //    description string A short description of the CustomsItem
        //    quantity    number The quantity for this line item
        //value number  The value (in USD) of the line item
        //harmonizedTariffCode string The Harmonized Commodity Code for this line item
        //countryOfOrigin string The two-letter ISO Origin Country code where the item originated


        public string CustomsItemId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Value { get; set; }
        public string HarmonizedTariffCode { get; set; }
        public string CountryOfOrigin { get; set; }
    }
}
