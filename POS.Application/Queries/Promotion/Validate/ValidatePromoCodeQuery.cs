using MediatR;
using POS.Application.DTOs;

namespace POS.Application.Queries.Promotion.Validate;

public record ValidatePromoCodeQuery(string Code, decimal CartAmount) : IRequest<ValidationResultDto>;

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public PromotionDto? Promotion { get; set; }
}
