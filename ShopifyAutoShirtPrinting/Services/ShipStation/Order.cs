namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class Order
    {
        public long OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderKey { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string CreateDate { get; set; }
        public string RequestedShippingService { get; set; }
        public string CarrierCode { get; set; }
        public string ServiceCode { get; set; }
        public string PackageCode { get; set; }
        public Weight Weight { get; set; }
        public long CustomerId { get; set; }



    }
}
