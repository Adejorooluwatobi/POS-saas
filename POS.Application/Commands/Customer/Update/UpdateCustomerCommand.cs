using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Customer.Update;

public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Dto) : IRequest;
