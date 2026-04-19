using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Commands.Promotion.Create;

public record CreatePromotionCommand(CreatePromotionDto Dto) : IRequest<PromotionDto>;
