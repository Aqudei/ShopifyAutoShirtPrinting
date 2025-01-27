namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class OrderLine
    {
        public long OrderItemId { get; set; }
        public string LineItemKey { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public Weight Weight { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
    }
}
