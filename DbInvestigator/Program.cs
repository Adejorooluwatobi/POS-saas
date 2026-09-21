using POS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

var builder = new ConfigurationBuilder().SetBasePath("c:\\Development\\POS_SAAS\\POS\\POS.Api").AddJsonFile("appsettings.json");
var configuration = builder.Build();
var optionsBuilder = new DbContextOptionsBuilder<RetailOsDbContext>();
optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

using var db = new RetailOsDbContext(optionsBuilder.Options);

var order = db.InventoryOrders.IgnoreQueryFilters().Include(o => o.Items).FirstOrDefault(o => o.Id == Guid.Parse("b936b379-6621-4eb9-a76f-611db22a824a"));
Console.WriteLine("Order ID: " + order.Id);
Console.WriteLine("Source: " + order.SourceStoreId);
Console.WriteLine("Dest: " + order.DestinationStoreId);

foreach (var item in order.Items)
{
    Console.WriteLine("Item: VariantId=" + item.VariantId + " Qty=" + item.QuantityOrdered);
}

var inventories = db.Inventories.IgnoreQueryFilters().Where(i => i.StoreId == order.DestinationStoreId).ToList();
foreach (var inv in inventories)
{
    Console.WriteLine("Inventory: VariantId=" + inv.VariantId + " Store=" + inv.StoreId + " Qty=" + inv.QuantityOnHand);
}
