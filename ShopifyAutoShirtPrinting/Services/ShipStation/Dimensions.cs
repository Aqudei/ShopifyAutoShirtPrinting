namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class Dimensions
    {
        //length number  Length of package.
        //    width number  Width of package.
        //    height number  Height of package.
        //    units string Units of measurement.Allowed units are: "inches", or "centimeters"

        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Units { get; set; }
    }
}
