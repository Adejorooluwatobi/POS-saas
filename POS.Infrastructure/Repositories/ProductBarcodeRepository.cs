using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class ProductBarcodeRepository : GenericRepository<ProductBarcode>, IProductBarcodeRepository
{
    public ProductBarcodeRepository(RetailOsDbContext context) : base(context) { }

    public async Task<List<ProductBarcode>> GetByVariantIdAsync(Guid variantId)
    {
        return await _context.ProductBarcodes
            .Where(b => b.VariantId == variantId)
            .ToListAsync();
    }

    public async Task<ProductBarcode?> GetByBarcodeAsync(string barcode)
    {
        return await _context.ProductBarcodes
            .FirstOrDefaultAsync(b => b.BarcodeValue == barcode);
    }
}
