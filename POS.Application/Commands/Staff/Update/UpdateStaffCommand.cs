using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Staff.Update;

public record UpdateStaffCommand(Guid Id, UpdateStaffDto Dto) : IRequest;
