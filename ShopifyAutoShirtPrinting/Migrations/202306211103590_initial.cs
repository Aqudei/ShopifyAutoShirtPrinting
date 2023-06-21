namespace ShopifyEasyShirtPrinting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChangeDate = c.DateTime(nullable: false),
                        ChangeStatus = c.String(),
                        MyLineItemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("MyLineItems", t => t.MyLineItemId, cascadeDelete: true)
                .Index(t => t.MyLineItemId);
            
            CreateTable(
                "MyLineItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderNumber = c.String(),
                        Sku = c.String(),
                        Name = c.String(),
                        VariantId = c.Long(),
                        VariantTitle = c.String(),
                        LineItemId = c.Long(),
                        Quantity = c.Int(),
                        FulfillmentStatus = c.String(),
                        FinancialStatus = c.String(),
                        Customer = c.String(),
                        CustomerEmail = c.String(),
                        DateModified = c.DateTime(),
                        ProductImage = c.String(),
                        Notes = c.String(),
                        OrderId = c.Long(),
                        PrintedQuantity = c.Int(nullable: false),
                        BinNumber = c.Int(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "OrderInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BinNumber = c.Int(nullable: false),
                        OrderId = c.Long(nullable: false),
                        Active = c.Boolean(nullable: false),
                        LabelPrinted = c.Boolean(nullable: false),
                        LabelData = c.String(),
                        TrackingNumber = c.String(),
                        InsuranceCost = c.Double(nullable: false),
                        ShipmentCost = c.Double(nullable: false),
                        ShipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Logs", "MyLineItemId", "MyLineItems");
            DropIndex("Logs", new[] { "MyLineItemId" });
            DropTable("OrderInfoes");
            DropTable("MyLineItems");
            DropTable("Logs");
        }
    }
}
