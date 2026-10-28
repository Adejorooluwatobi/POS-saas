using System;
using MediatR;

namespace POS.Application.Queries.Tenant.ExportData;

public class TenantExportResultDto
{
    public string FileName { get; set; } = null!;
    public byte[] CsvBytes { get; set; } = null!;
    public string ContentType { get; set; } = "text/csv";
}

public record ExportTenantDataQuery(Guid Id) : IRequest<TenantExportResultDto>;
