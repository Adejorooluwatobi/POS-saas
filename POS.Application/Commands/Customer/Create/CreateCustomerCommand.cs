using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Customer.Create;

public record CreateCustomerCommand(CreateCustomerDto Dto) : IRequest<CustomerDto>;
