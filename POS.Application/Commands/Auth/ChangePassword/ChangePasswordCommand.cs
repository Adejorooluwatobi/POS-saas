using System;
using MediatR;

namespace POS.Application.Commands.Auth.ChangePassword;

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string? NewPin { get; set; }
}

public class ChangePasswordResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
}

public record ChangePasswordCommand(ChangePasswordDto Dto) : IRequest<ChangePasswordResultDto>;
