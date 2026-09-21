using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using POS.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

var config = new ConfigurationBuilder()
    .SetBasePath(Path.GetFullPath(""../POS.Api""))
    .AddJsonFile(""appsettings.json"")
    .Build();

var options = new DbContextOptionsBuilder<RetailOsDbContext>()
    .UseNpgsql(config.GetConnectionString(""DefaultConnection""))
    .Options;

using var context = new RetailOsDbContext(options, null); // ITenantContext is null

var orders = context.Set<POS.Domain.Entities.InventoryOrder>().ToList();
Console.WriteLine($""Total Orders: {orders.Count}"");
foreach(var o in orders) {
    Console.WriteLine($""Order {o.OrderNumber} - Tenant: {o.TenantId} - Dest: {o.DestinationStoreId} - Src: {o.SourceStoreId}"");
}

var staff = context.Set<POS.Domain.Entities.Staff>().ToList();
Console.WriteLine($""Total Staff: {staff.Count}"");
foreach(var s in staff) {
    Console.WriteLine($""Staff {s.Email} - Tenant: {s.TenantId} - Store: {s.StoreId} - Role: {s.SystemRole}"");
}

var stores = context.Set<POS.Domain.Entities.Store>().ToList();
Console.WriteLine($""Total Stores: {stores.Count}"");
foreach(var s in stores) {
    Console.WriteLine($""Store {s.Name} - ID: {s.Id} - Tenant: {s.TenantId}"");
}
