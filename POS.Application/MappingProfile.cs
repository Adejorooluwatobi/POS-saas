using System.Text.Json;
using AutoMapper;
using POS.Domain.Entities;
using POS.Application.DTOs;

namespace POS.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Tenant ────────────────────────────────────────────────────────
        CreateMap<Tenant, TenantDto>();
        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<UpdateTenantDto, Tenant>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Slug, o => o.Ignore());

        // ── Store ─────────────────────────────────────────────────────────
        CreateMap<Store, StoreDto>();
        CreateMap<CreateStoreDto, Store>()
            .ForMember(d => d.TenantId, o => o.Ignore());
        CreateMap<UpdateStoreDto, Store>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Code, o => o.Ignore());

        // ── Role ──────────────────────────────────────────────────────────
        CreateMap<Role, RoleDto>()
            .ForMember(d => d.Permissions, o => o.MapFrom(s =>
                s.Permissions != null
                    ? JsonSerializer.Deserialize<Dictionary<string, bool>>(s.Permissions.RootElement.GetRawText())
                    : new Dictionary<string, bool>()));
        CreateMap<CreateRoleDto, Role>()
            .ForMember(d => d.Permissions, o => o.MapFrom(s =>
                JsonDocument.Parse(JsonSerializer.Serialize(s.Permissions))));
        CreateMap<UpdateRoleDto, Role>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Permissions, o => o.MapFrom(s =>
                JsonDocument.Parse(JsonSerializer.Serialize(s.Permissions))));

        // ── Staff ─────────────────────────────────────────────────────────
        CreateMap<Staff, StaffDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));
        CreateMap<CreateStaffDto, Staff>()
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.PinHash, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore());
        CreateMap<UpdateStaffDto, Staff>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.EmployeeNo, o => o.Ignore())
            .ForMember(d => d.PinHash, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.HiredAt, o => o.Ignore());

        // ── Customer ──────────────────────────────────────────────────────
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>()
            .ForMember(d => d.TenantId, o => o.Ignore());
        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.LoyaltyCardNo, o => o.Ignore())
            .ForMember(d => d.PointsBalance, o => o.Ignore())
            .ForMember(d => d.Tier, o => o.Ignore());

        // ── Category ──────────────────────────────────────────────────────
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore());

        // ── Product ───────────────────────────────────────────────────────
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.MasterSku, o => o.Ignore());

        // ── Promotion ─────────────────────────────────────────────────────
        CreateMap<Promotion, PromotionDto>();
        CreateMap<CreatePromotionDto, Promotion>()
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Conditions, o => o.Ignore());
        CreateMap<UpdatePromotionDto, Promotion>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Type, o => o.Ignore())
            .ForMember(d => d.Scope, o => o.Ignore())
            .ForMember(d => d.Conditions, o => o.Ignore())
            .ForMember(d => d.UsedCount, o => o.Ignore());

        // ── TillSession ───────────────────────────────────────────────────
        CreateMap<TillSession, TillSessionDto>();
        CreateMap<CreateTillSessionDto, TillSession>()
            .ForMember(d => d.StaffId, o => o.Ignore());
        CreateMap<UpdateTillSessionDto, TillSession>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TerminalId, o => o.Ignore())
            .ForMember(d => d.StaffId, o => o.Ignore())
            .ForMember(d => d.OpenedAt, o => o.Ignore())
            .ForMember(d => d.OpeningFloat, o => o.Ignore());

        // ── Inventory ─────────────────────────────────────────────────────
        CreateMap<Inventory, InventoryDto>()
            .ForMember(d => d.QuantityAvailable, o => o.MapFrom(s => s.QuantityAvailable));
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.VariantId, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore());

        // ── Payment ───────────────────────────────────────────────────────
        CreateMap<Payment, PaymentDto>();
        CreateMap<CreatePaymentDto, Payment>()
            .ForMember(d => d.ChangeGiven, o => o.Ignore())
            .ForMember(d => d.GatewayRef, o => o.Ignore())
            .ForMember(d => d.GatewayResponse, o => o.Ignore())
            .ForMember(d => d.ProcessedAt, o => o.Ignore());
        CreateMap<UpdatePaymentDto, Payment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransactionId, o => o.Ignore())
            .ForMember(d => d.Method, o => o.Ignore())
            .ForMember(d => d.Amount, o => o.Ignore());

        // ── Transaction ───────────────────────────────────────────────────
        CreateMap<TransactionItem, TransactionItemDto>();
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.Payments, o => o.MapFrom(s => s.Payments));
        CreateMap<CreateTransactionDto, Transaction>()
            .ForMember(d => d.ReceiptNumber, o => o.Ignore())
            .ForMember(d => d.CashierId, o => o.Ignore())
            .ForMember(d => d.Subtotal, o => o.Ignore())
            .ForMember(d => d.DiscountTotal, o => o.Ignore())
            .ForMember(d => d.TaxTotal, o => o.Ignore())
            .ForMember(d => d.GrandTotal, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.ChangeGiven, o => o.Ignore())
            .ForMember(d => d.PointsEarned, o => o.Ignore())
            .ForMember(d => d.PointsRedeemed, o => o.Ignore())
            .ForMember(d => d.Items, o => o.Ignore());
        CreateMap<UpdateTransactionDto, Transaction>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ReceiptNumber, o => o.Ignore())
            .ForMember(d => d.SessionId, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore())
            .ForMember(d => d.CashierId, o => o.Ignore())
            .ForMember(d => d.CustomerId, o => o.Ignore())
            .ForMember(d => d.VoidRefId, o => o.Ignore())
            .ForMember(d => d.Type, o => o.Ignore())
            .ForMember(d => d.Subtotal, o => o.Ignore())
            .ForMember(d => d.DiscountTotal, o => o.Ignore())
            .ForMember(d => d.TaxTotal, o => o.Ignore())
            .ForMember(d => d.GrandTotal, o => o.Ignore())
            .ForMember(d => d.AmountPaid, o => o.Ignore())
            .ForMember(d => d.ChangeGiven, o => o.Ignore())
            .ForMember(d => d.PointsEarned, o => o.Ignore())
            .ForMember(d => d.PointsRedeemed, o => o.Ignore())
            .ForMember(d => d.CompletedAt, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Items, o => o.Ignore())
            .ForMember(d => d.Payments, o => o.Ignore())
            .ForMember(d => d.AppliedDiscounts, o => o.Ignore());

        // ── Terminal ──────────────────────────────────────────────────────
        CreateMap<Terminal, TerminalDto>();
        CreateMap<CreateTerminalDto, Terminal>()
            .ForMember(d => d.StoreId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.LastPingAt, o => o.Ignore());
        CreateMap<UpdateTerminalDto, Terminal>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore())
            .ForMember(d => d.TerminalCode, o => o.Ignore());

        // ── Coupon ────────────────────────────────────────────────────────
        CreateMap<Coupon, CouponDto>();
        CreateMap<CreateCouponDto, Coupon>()
            .ForMember(d => d.PromotionId, o => o.Ignore())
            .ForMember(d => d.UsedCount, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore());
        CreateMap<UpdateCouponDto, Coupon>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PromotionId, o => o.Ignore())
            .ForMember(d => d.Code, o => o.Ignore())
            .ForMember(d => d.UsedCount, o => o.Ignore())
            .ForMember(d => d.SingleUsePerCustomer, o => o.Ignore());

        // ── GiftCard ──────────────────────────────────────────────────────
        CreateMap<GiftCard, GiftCardDto>();
        CreateMap<IssueGiftCardDto, GiftCard>()
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Balance, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.IssuedAt, o => o.Ignore());

        // ── TenantSubscription ────────────────────────────────────────────
        CreateMap<TenantSubscription, TenantSubscriptionDto>()
            .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.CurrentPeriodEnd < DateTimeOffset.UtcNow));
        CreateMap<UpdateSubscriptionDto, TenantSubscription>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.MaxStores, o => o.Ignore())
            .ForMember(d => d.MaxTerminals, o => o.Ignore())
            .ForMember(d => d.MaxStaff, o => o.Ignore())
            .ForMember(d => d.MonthlyPrice, o => o.Ignore())
            .ForMember(d => d.TrialEndsAt, o => o.Ignore())
            .ForMember(d => d.CurrentPeriodStart, o => o.Ignore())
            .ForMember(d => d.CurrentPeriodEnd, o => o.Ignore())
            .ForMember(d => d.CancelledAt, o => o.Ignore());

        // ── AuditLog ──────────────────────────────────────────────────────
        CreateMap<AuditLog, AuditLogDto>();
    }
}
