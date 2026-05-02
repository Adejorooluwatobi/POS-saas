using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Domain.Entities;

namespace POS.Domain.Repositories;

public interface IProductBarcodeRepository : IGenericRepository<ProductBarcode>
{
    Task<List<ProductBarcode>> GetByVariantIdAsync(Guid variantId);
    Task<ProductBarcode?> GetByBarcodeAsync(string barcode);
}
