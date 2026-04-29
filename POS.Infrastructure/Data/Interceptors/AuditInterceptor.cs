using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Tenancy;
using POS.Domain.Interfaces;

namespace POS.Infrastructure.Data.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public AuditInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = OnBeforeSaveChanges(eventData.Context);
        if (auditEntries.Any())
        {
            eventData.Context.Set<AuditLog>().AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> OnBeforeSaveChanges(DbContext context)
    {
        context.ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditableAttribute = entry.Entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), true).FirstOrDefault();
            if (auditableAttribute == null)
                continue;

            var auditEntry = new AuditLog
            {
                TenantId = _tenantContext.TenantId,
                StoreId = _tenantContext.StoreId,
                UserId = _tenantContext.UserId,
                Action = entry.State switch
                {
                    EntityState.Added => AuditAction.Insert,
                    EntityState.Deleted => AuditAction.Delete,
                    EntityState.Modified => AuditAction.Update,
                    _ => AuditAction.Update
                },
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKeyValue(entry),
                ActorMetadata = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    actorId = _tenantContext.UserId,
                    actorName = _tenantContext.UserName,
                    actorRole = _tenantContext.SystemRole
                })),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var changes = new Dictionary<string, object?>();

            if (entry.State == EntityState.Added)
            {
                foreach (var prop in entry.Properties)
                {
                    changes[prop.Metadata.Name] = prop.CurrentValue;
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                foreach (var prop in entry.Properties)
                {
                    changes[prop.Metadata.Name] = prop.OriginalValue;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var diff = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties.Where(p => p.IsModified))
                {
                    diff[prop.Metadata.Name] = new
                    {
                        Old = prop.OriginalValue,
                        New = prop.CurrentValue
                    };
                }
                changes = diff;
            }

            if (changes.Any())
            {
                auditEntry.Changes = JsonDocument.Parse(JsonSerializer.Serialize(changes));
                auditEntries.Add(auditEntry);
            }
        }

        return auditEntries;
    }

    private string GetPrimaryKeyValue(EntityEntry entry)
    {
        var pk = entry.Metadata.FindPrimaryKey();
        if (pk == null) return "Unknown";
        
        var values = pk.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString());
        return string.Join(",", values);
    }
}
