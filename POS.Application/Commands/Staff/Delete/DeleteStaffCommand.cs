using System;
using MediatR;

namespace POS.Application.Commands.Staff.Delete;

public record DeleteStaffCommand(Guid Id) : IRequest;
