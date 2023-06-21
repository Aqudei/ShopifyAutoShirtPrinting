using ShopifyEasyShirtPrinting.Models;
using System.Data.Entity;

namespace ShopifyEasyShirtPrinting.Data
{
    [DbConfigurationType(typeof(NpgSqlConfiguration))]
    public class LonelyKidsContext : DbContext
    {
        public virtual DbSet<MyLineItem> MyLineItems { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<OrderInfo> OrderInfos { get; set; }


        public LonelyKidsContext() : base("name=LonelyKidsContext")
        { }
        public LonelyKidsContext(string nameOrConnectionString) : base(nameOrConnectionString)
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(string.Empty);

            modelBuilder.Entity<MyLineItem>()
                .HasKey(x => x.Id)
                .Ignore(x => x.IsSelected);


            modelBuilder.Entity<Log>()
                .HasKey(x => x.Id)
                .HasRequired(x => x.MyLineItem)
                .WithMany().
                HasForeignKey(x => x.MyLineItemId);


            modelBuilder.Entity<OrderInfo>()
                .HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }

    }
}
