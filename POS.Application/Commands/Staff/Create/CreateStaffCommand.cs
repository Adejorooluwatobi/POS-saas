using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Staff.Create;

public record CreateStaffCommand(CreateStaffDto Dto) : IRequest<StaffDto>;
