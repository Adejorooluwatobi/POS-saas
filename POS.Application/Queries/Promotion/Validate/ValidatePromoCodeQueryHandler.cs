using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;

namespace POS.Application.Queries.Promotion.Validate;

public class ValidatePromoCodeQueryHandler : IRequestHandler<ValidatePromoCodeQuery, ValidationResultDto>
{
    private readonly IPromotionRepository _promotionRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    public ValidatePromoCodeQueryHandler(
        IPromotionRepository promotionRepo,
        ICouponRepository couponRepo,
        ITenantContext tenantContext,
        IMapper mapper)
    {
        _promotionRepo = promotionRepo;
        _couponRepo = couponRepo;
        _tenantContext = tenantContext;
        _mapper = mapper;
    }

    public async Task<ValidationResultDto> Handle(ValidatePromoCodeQuery request, CancellationToken cancellationToken)
    {
        POS.Domain.Entities.Promotion? promotion = null;
        POS.Domain.Entities.Coupon? coupon = null;

        // 1. Try to find a global promotion with this code
        promotion = await _promotionRepo.GetQueryable()
            .Include(p => p.Coupons)
            .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

        // 2. If not found, try to find a specific coupon code
        if (promotion == null)
        {
            coupon = await _couponRepo.GetQueryable()
                .Include(c => c.Promotion)
                .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);
            
            if (coupon != null)
            {
                promotion = coupon.Promotion;
            }
        }

        if (promotion == null)
        {
            return new ValidationResultDto { IsValid = false, Message = "Invalid promo code." };
        }

        // 3. Basic Validation
        if (!promotion.IsActive)
            return new ValidationResultDto { IsValid = false, Message = "This promotion is no longer active." };

        var now = DateTimeOffset.UtcNow;
        if (promotion.StartsAt > now)
            return new ValidationResultDto { IsValid = false, Message = "This promotion has not started yet." };

        if (promotion.EndsAt.HasValue && promotion.EndsAt < now)
            return new ValidationResultDto { IsValid = false, Message = "This promotion has expired." };

        // 4. Scoping Validation (Inherited Redemption)
        // If promotion.StoreId is null, it's global (redeemable anywhere).
        // If set, it must match the terminal's store.
        if (promotion.StoreId.HasValue && promotion.StoreId != _tenantContext.StoreId)
        {
            return new ValidationResultDto { IsValid = false, Message = "This promo code is not valid for this store location." };
        }

        // 5. Usage Limits
        if (coupon != null)
        {
            if (coupon.UsedCount >= coupon.MaxUses)
                return new ValidationResultDto { IsValid = false, Message = "This coupon code has reached its maximum usage limit." };
        }
        else if (promotion.MaxUses.HasValue && promotion.UsedCount >= promotion.MaxUses)
        {
            return new ValidationResultDto { IsValid = false, Message = "This promo code has reached its maximum usage limit." };
        }

        // 6. Purchase Requirements
        if (promotion.MinPurchase.HasValue && request.CartAmount < promotion.MinPurchase.Value)
        {
            return new ValidationResultDto 
            { 
                IsValid = false, 
                Message = $"Minimum purchase of {promotion.MinPurchase:C} required to use this code." 
            };
        }

        // 7. Calculate Discount
        decimal discount = 0;
        if (promotion.Type == PromotionType.Percent)
        {
            discount = request.CartAmount * (promotion.Value / 100);
            if (promotion.MaxDiscount.HasValue)
                discount = Math.Min(discount, promotion.MaxDiscount.Value);
        }
        else if (promotion.Type == PromotionType.Fixed)
        {
            discount = Math.Min(promotion.Value, request.CartAmount);
        }

        return new ValidationResultDto
        {
            IsValid = true,
            Message = "Promo code applied successfully!",
            DiscountAmount = discount,
            Promotion = _mapper.Map<PromotionDto>(promotion)
        };
    }
}
