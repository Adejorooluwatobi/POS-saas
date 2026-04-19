using System;
using MediatR;

namespace POS.Application.Commands.Customer.Delete;

public record DeleteCustomerCommand(Guid Id) : IRequest;
