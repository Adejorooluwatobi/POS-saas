using System;
using MediatR;

namespace POS.Application.Commands.Staff.Restore;

public record RestoreStaffCommand(Guid Id) : IRequest;
