using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Tenant.GetById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;
