using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Promotion.GetById;

public record GetPromotionByIdQuery(Guid Id) : IRequest<PromotionDto?>;
