using System;

namespace POS.Domain.Common;

[AttributeUsage(AttributeTargets.Class)]
public class AuditableAttribute : Attribute { }
