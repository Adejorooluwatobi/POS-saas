using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Tenant.Update;

public record UpdateTenantCommand(Guid Id, UpdateTenantDto Dto) : IRequest;
