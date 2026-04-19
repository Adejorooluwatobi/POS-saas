using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Commands.Auth.LoginAdmin;
using POS.Application.Commands.Auth.LoginCustomer;
using POS.Application.Commands.Auth.LoginPos;
using POS.Application.Commands.Auth.RegisterCustomer;
using POS.Application.Commands.Auth.RegisterTenant;
using POS.Application.DTOs.Auth;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Registers a new Business Tenant on the platform along with the initial Admin user.</summary>
    [HttpPost("register-tenant")]
    public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantDto dto)
    {
        var result = await _mediator.Send(new RegisterTenantCommand(dto));
        return Ok(result);
    }

    /// <summary>Registers a new Consumer for loyalty and digital receipts.</summary>
    [HttpPost("register-customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
    {
        var result = await _mediator.Send(new RegisterCustomerCommand(dto));
        return Ok(result);
    }

    /// <summary>Global Dashboard Login for Super Admins and passed Tenant Admins via Email/Password.</summary>
    [HttpPost("login-admin")]
    public async Task<IActionResult> LoginAdmin([FromBody] AdminLoginDto dto)
    {
        var result = await _mediator.Send(new LoginAdminCommand(dto));
        return Ok(result);
    }

    /// <summary>Cashier Login from the physical POS Terminal using Employee No and PIN.</summary>
    [HttpPost("login-pos")]
    public async Task<IActionResult> LoginPos([FromBody] PosLoginDto dto)
    {
        var result = await _mediator.Send(new LoginPosCommand(dto));
        return Ok(result);
    }

    /// <summary>Consumer Login for the loyalty portal.</summary>
    [HttpPost("login-customer")]
    public async Task<IActionResult> LoginCustomer([FromBody] CustomerLoginDto dto)
    {
        var result = await _mediator.Send(new LoginCustomerCommand(dto));
        return Ok(result);
    }
}
