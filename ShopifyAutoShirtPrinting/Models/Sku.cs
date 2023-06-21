namespace ShopifyEasyShirtPrinting.Models
{
    public class Sku
    {
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string ProductColour { get; set; }
        public string ProductFit { get; set; }
        public string ProductSize { get; set; }

        public static Sku Parse(string input)
        {
            var parts = input.Split(' ');
            if (parts.Length < 5) return null;

            var sku = new Sku
            {
                ProductType = parts[0],
                ProductName = parts[1],
                ProductColour = parts[2],
                ProductFit = parts[3],
                ProductSize = parts[4]
            };

            return sku;
        }
    }
}
