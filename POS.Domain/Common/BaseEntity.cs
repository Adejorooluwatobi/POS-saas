using System;

namespace POS.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
