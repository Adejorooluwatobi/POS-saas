using System;
using System.Collections.Generic;
using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Category : BaseEntity
{
    public required Guid TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public short Depth { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
