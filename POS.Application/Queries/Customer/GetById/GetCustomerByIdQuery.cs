using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Customer.GetById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;
