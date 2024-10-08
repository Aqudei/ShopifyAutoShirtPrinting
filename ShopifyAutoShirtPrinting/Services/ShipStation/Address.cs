﻿namespace ShopifyEasyShirtPrinting.Services.ShipStation
{
    public class Address
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string Street3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public bool Residential { get; set; }
        public bool AddressVerified { get; set; }
    }
}
