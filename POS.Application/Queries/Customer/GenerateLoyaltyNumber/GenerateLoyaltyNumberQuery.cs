using System;
using MediatR;

namespace POS.Application.Queries.Customer.GenerateLoyaltyNumber;

public record GenerateLoyaltyNumberQuery(Guid? StoreId = null) : IRequest<string>;
