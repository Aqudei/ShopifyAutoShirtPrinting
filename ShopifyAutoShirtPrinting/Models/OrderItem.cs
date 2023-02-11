namespace ShopifyEasyShirtPrinting.Models
{
    public class OrderItem
    {
        public bool IsSelected { get; set; }
        public int? OrderNumber { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public long? VariantId { get; set; }
        public string VariantTitle { get; set; }
        public long? LineItemId { get; set; }
        public int? Quantity { get; set; }
        public string FulfillmentStatus { get; set; }
        public string FinancialStatus { get; set; }
        public string Customer { get; set; }
        public string CustomerEmail { get; set; }
        public string ProductImage { get; set; }
    }
}
