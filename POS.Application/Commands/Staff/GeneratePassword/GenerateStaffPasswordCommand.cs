using System;
using MediatR;
using POS.Domain.Enums;

namespace POS.Application.Commands.Staff.GeneratePassword;

public class GeneratedCredentialsDto
{
    public Guid StaffId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string GeneratedPassword { get; set; } = null!;
    public string? GeneratedPin { get; set; }
    public SystemRole SystemRole { get; set; }
    public string Message { get; set; } = null!;
}

public record GenerateStaffPasswordCommand(Guid StaffId) : IRequest<GeneratedCredentialsDto>;
