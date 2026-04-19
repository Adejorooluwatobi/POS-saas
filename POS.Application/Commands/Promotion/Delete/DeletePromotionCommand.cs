using System;
using MediatR;

namespace POS.Application.Commands.Promotion.Delete;

public record DeletePromotionCommand(Guid Id) : IRequest;
