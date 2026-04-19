using System;
using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Promotion.Update;

public record UpdatePromotionCommand(Guid Id, UpdatePromotionDto Dto) : IRequest;
