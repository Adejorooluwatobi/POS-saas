using System;
using MediatR;

namespace POS.Application.Commands.Staff.Purge;

public record PurgeStaffCommand(Guid Id) : IRequest;
