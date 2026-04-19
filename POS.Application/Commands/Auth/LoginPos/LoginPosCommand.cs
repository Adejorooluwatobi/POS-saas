using MediatR;
using POS.Application.DTOs.Auth;

namespace POS.Application.Commands.Auth.LoginPos;

public record LoginPosCommand(PosLoginDto Dto) : IRequest<AuthResponseDto>;
