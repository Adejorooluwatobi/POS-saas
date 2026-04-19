using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Staff.GetById;

public record GetStaffByIdQuery(Guid Id) : IRequest<StaffDto?>;
