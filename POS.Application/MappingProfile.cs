using System.Text.Json;
using AutoMapper;
using POS.Domain.Common;
using POS.Domain.Entities;
using POS.Domain.Interfaces;
using POS.Application.DTOs;
using POS.Application.DTOs.InventoryOrder;
using POS.Application.DTOs.StockRequisition;

namespace POS.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Tenant ────────────────────────────────────────────────────────
        CreateMap<Tenant, TenantDto>()
            .ForMember(d => d.OwnerEmail, o => o.MapFrom(s => 
                s.Staff.FirstOrDefault(st => st.SystemRole == POS.Domain.Enums.SystemRole.TenantAdmin) != null 
                ? s.Staff.FirstOrDefault(st => st.SystemRole == POS.Domain.Enums.SystemRole.TenantAdmin)!.Email 
                : null));
        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<UpdateTenantDto, Tenant>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Slug, o => o.Ignore());

        // ── Store ─────────────────────────────────────────────────────────
        CreateMap<Store, StoreDto>()
            .ForMember(d => d.TenantEmail, o => o.MapFrom(s => s.Tenant != null ? s.Tenant.ContactEmail : null));
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
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.HasPin, o => o.MapFrom(s => !string.IsNullOrEmpty(s.PinHash)))
            .ForMember(d => d.HasPassword, o => o.MapFrom(s => !string.IsNullOrEmpty(s.PasswordHash)));
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
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.MaskedIdentityNumber, o => o.MapFrom<MaskedIdentityResolver>())
            .ForMember(d => d.RegisteredStoreName, o => o.MapFrom(s => s.RegisteredStore != null ? s.RegisteredStore.Name : (s.IsSelfRegistered ? "Online (Self-Registered)" : null)))
            .ForMember(d => d.RegisteredByStaffName, o => o.MapFrom(s => s.RegisteredByStaff != null ? s.RegisteredByStaff.FullName : null))
            .ForMember(d => d.TotalSpend, o => o.MapFrom(s => s.Transactions.Where(t => t.Status == POS.Domain.Enums.TransactionStatus.Completed).Sum(t => t.GrandTotal)))
            .ForMember(d => d.TotalVisits, o => o.MapFrom(s => s.Transactions.Count(t => t.Status == POS.Domain.Enums.TransactionStatus.Completed)));
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
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.BasePrice, o => o.MapFrom(s => s.Variants.FirstOrDefault() != null ? s.Variants.FirstOrDefault()!.BasePrice : 0))
            .ForMember(d => d.CostPrice, o => o.MapFrom(s => s.Variants.FirstOrDefault() != null ? s.Variants.FirstOrDefault()!.CostPrice : 0))
            .ForMember(d => d.WeightGrams, o => o.MapFrom(s => s.Variants.FirstOrDefault() != null ? s.Variants.FirstOrDefault()!.WeightGrams : null))
            .ForMember(d => d.UnitOfMeasure, o => o.MapFrom(s => s.Variants.FirstOrDefault() != null ? s.Variants.FirstOrDefault()!.UnitOfMeasure : "Each"))
            .ForMember(d => d.Barcodes, o => o.MapFrom(s => s.Variants.FirstOrDefault() != null ? s.Variants.FirstOrDefault()!.Barcodes.Select(b => b.BarcodeValue).ToList() : new List<string>()))
            .ForMember(d => d.StoreOverrides, o => o.MapFrom(s => s.StoreOverrides))
            .ForMember(d => d.Variants, o => o.MapFrom(s => s.Variants));
        CreateMap<ProductVariant, ProductVariantDto>();
        CreateMap<StoreProductOverride, StoreProductOverrideDto>();
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
        CreateMap<TillSession, TillSessionDto>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.OpenedAt));
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
            .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant != null && s.Variant.Product != null ? s.Variant.Product.Name : string.Empty))
            .ForMember(d => d.Sku, o => o.MapFrom(s => s.Variant != null ? s.Variant.Sku : string.Empty))
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store != null ? s.Store.Name : string.Empty))
            .ForMember(d => d.QuantityAvailable, o => o.MapFrom(s => s.QuantityAvailable))
            .ForMember(d => d.SinglesPerRoll, o => o.MapFrom(s => s.Variant != null && s.Variant.Product != null ? s.Variant.Product.SinglesPerRoll : null))
            .ForMember(d => d.RollsPerPack, o => o.MapFrom(s => s.Variant != null && s.Variant.Product != null ? s.Variant.Product.RollsPerPack : null))
            .ForMember(d => d.SinglesPerPack, o => o.MapFrom(s => s.Variant != null && s.Variant.Product != null ? s.Variant.Product.SinglesPerPack : null));
        CreateMap<AggregatedInventory, InventoryDto>();
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.VariantId, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore());

        // ── Payment ───────────────────────────────────────────────────────
        CreateMap<POS.Domain.Entities.Payment, PaymentDto>();
        CreateMap<CreatePaymentDto, POS.Domain.Entities.Payment>()
            .ForMember(d => d.ChangeGiven, o => o.Ignore())
            .ForMember(d => d.GatewayRef, o => o.Ignore())
            .ForMember(d => d.GatewayResponse, o => o.Ignore())
            .ForMember(d => d.ProcessedAt, o => o.Ignore());
        CreateMap<UpdatePaymentDto, POS.Domain.Entities.Payment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TransactionId, o => o.Ignore())
            .ForMember(d => d.Method, o => o.Ignore())
            .ForMember(d => d.Amount, o => o.Ignore());

        // ── Transaction ───────────────────────────────────────────────────
        CreateMap<TransactionItem, TransactionItemDto>()
            .ForMember(d => d.VariantName, o => o.MapFrom(s => 
                s.ProductName ?? (s.Variant != null ? (s.Variant.Product != null ? s.Variant.Product.Name : s.Variant.Sku) : "Unknown")));
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.Payments, o => o.MapFrom(s => s.Payments))
            .ForMember(d => d.CashierName, o => o.MapFrom(s => s.Cashier != null ? s.Cashier.FullName : "Unknown"))
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store != null ? s.Store.Name : "Unknown"));
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
            .ForMember(d => d.Items, o => o.Ignore())
            .ForMember(d => d.Payments, o => o.Ignore());
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
        CreateMap<Terminal, TerminalDto>()
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store.Name))
            .ForMember(d => d.StoreAddress, o => o.MapFrom(s => s.Store.Address))
            .ForMember(d => d.StoreCity, o => o.MapFrom(s => s.Store.City))
            .ForMember(d => d.StorePhone, o => o.MapFrom(s => s.Store.Phone))
            .ForMember(d => d.TenantEmail, o => o.MapFrom(s => s.Store != null && s.Store.Tenant != null ? s.Store.Tenant.ContactEmail : null));
        CreateMap<CreateTerminalDto, Terminal>()
            .ForMember(d => d.TerminalCode, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.LastPingAt, o => o.Ignore())
            .ForMember(d => d.PairingCode, o => o.Ignore())
            .ForMember(d => d.DeviceToken, o => o.Ignore())
            .ForMember(d => d.PairingCodeExpiresAt, o => o.Ignore());
        CreateMap<UpdateTerminalDto, Terminal>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.StoreId, o => o.Ignore())
            .ForMember(d => d.TerminalCode, o => o.Ignore())
            .ForMember(d => d.PairingCode, o => o.Ignore())
            .ForMember(d => d.DeviceToken, o => o.Ignore())
            .ForMember(d => d.PairingCodeExpiresAt, o => o.Ignore());

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
        CreateMap<GiftCard, GiftCardDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : null))
            .ForMember(d => d.CustomerPhone, o => o.MapFrom(s => s.Customer != null ? s.Customer.Phone : null))
            .ForMember(d => d.CustomerEmail, o => o.MapFrom(s => s.Customer != null ? s.Customer.Email : null))
            .ForMember(d => d.CustomerLoyaltyCardNo, o => o.MapFrom(s => s.Customer != null ? s.Customer.LoyaltyCardNo : null))
            .ForMember(d => d.CustomerPointsBalance, o => o.MapFrom(s => s.Customer != null ? (int?)s.Customer.PointsBalance : null));
        CreateMap<IssueGiftCardDto, GiftCard>()
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Balance, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.IssuedAt, o => o.Ignore())
            .ForMember(d => d.Customer, o => o.Ignore())
            .ForMember(d => d.Transactions, o => o.Ignore());

        CreateMap<GiftCardTransaction, GiftCardTransactionDto>()
            .ForMember(d => d.CardNumber, o => o.MapFrom(s => s.GiftCard != null ? s.GiftCard.CardNumber : ""))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.Method.ToString()))
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store != null ? s.Store.Name : null))
            .ForMember(d => d.StaffName, o => o.MapFrom(s => s.Staff != null ? s.Staff.FullName : null));

        // ── TenantSubscription ────────────────────────────────────────────
        CreateMap<TenantSubscription, TenantSubscriptionDto>()
            .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.CurrentPeriodEnd < DateTimeOffset.UtcNow));
        CreateMap<UpdateSubscriptionDto, TenantSubscription>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.TrialEndsAt, o => o.Ignore())
            .ForMember(d => d.CurrentPeriodStart, o => o.Ignore())
            .ForMember(d => d.CancelledAt, o => o.Ignore());

        // ── AuditLog ──────────────────────────────────────────────────────
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.StaffName, o => o.MapFrom(s => s.User != null ? s.User.FullName : "System"))
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store != null ? s.Store.Name : "Global"))
            .ForMember(d => d.TerminalName, o => o.MapFrom(s => s.Terminal != null ? s.Terminal.Label ?? s.Terminal.TerminalCode : null))
            .ForMember(d => d.Changes, o => o.MapFrom(s => s.Changes != null ? s.Changes.RootElement.GetRawText() : null));

        // ── InventoryOrder ────────────────────────────────────────────────
        CreateMap<InventoryOrder, InventoryOrderDto>()
            .ForMember(d => d.SourceStoreName, o => o.MapFrom(s => s.SourceStore != null ? s.SourceStore.Name : "HQ"))
            .ForMember(d => d.DestinationStoreName, o => o.MapFrom(s => s.DestinationStore.Name))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy.FullName))
            .ForMember(d => d.ReceivedByName, o => o.MapFrom(s => s.ReceivedBy != null ? s.ReceivedBy.FullName : null))
            .ForMember(d => d.ApprovedByName, o => o.MapFrom(s => s.ApprovedBy != null ? s.ApprovedBy.FullName : null))
            .ForMember(d => d.ResolvedByName, o => o.MapFrom(s => s.ResolvedBy != null ? s.ResolvedBy.FullName : null));

        CreateMap<InventoryOrderItem, InventoryOrderItemDto>()
            .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.Name : s.Variant.Sku))
            .ForMember(d => d.Sku, o => o.MapFrom(s => s.Variant.Sku))
            .ForMember(d => d.ConversionFactor, o => o.MapFrom(s => 
                (s.Variant.ConversionFactor > 1) ? s.Variant.ConversionFactor : 
                (s.Variant.Product != null && s.Variant.Product.SinglesPerPack > 1 ? (decimal)s.Variant.Product.SinglesPerPack.Value : 1m)))
            .ForMember(d => d.SinglesPerRoll, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.SinglesPerRoll : null))
            .ForMember(d => d.RollsPerPack, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.RollsPerPack : null))
            .ForMember(d => d.SinglesPerPack, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.SinglesPerPack : null));

        // ── StockRequisition ──────────────────────────────────────────────
        CreateMap<StockRequisition, StockRequisitionDto>()
            .ForMember(d => d.RequestingStoreName, o => o.MapFrom(s => s.RequestingStore.Name))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy.FullName))
            .ForMember(d => d.ReviewedByName, o => o.MapFrom(s => s.ReviewedBy != null ? s.ReviewedBy.FullName : null));

        CreateMap<StockRequisitionItem, StockRequisitionItemDto>()
            .ForMember(d => d.VariantName, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.Name : s.Variant.Sku))
            .ForMember(d => d.Sku, o => o.MapFrom(s => s.Variant.Sku))
            .ForMember(d => d.ConversionFactor, o => o.MapFrom(s => 
                (s.Variant.ConversionFactor > 1) ? s.Variant.ConversionFactor : 
                (s.Variant.Product != null && s.Variant.Product.SinglesPerPack > 1 ? (decimal)s.Variant.Product.SinglesPerPack.Value : 1m)))
            .ForMember(d => d.SinglesPerRoll, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.SinglesPerRoll : null))
            .ForMember(d => d.RollsPerPack, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.RollsPerPack : null))
            .ForMember(d => d.SinglesPerPack, o => o.MapFrom(s => s.Variant.Product != null ? s.Variant.Product.SinglesPerPack : null));
    }
}

public class MaskedIdentityResolver : IValueResolver<Customer, CustomerDto, string?>
{
    private readonly IEncryptionService? _encryptionService;

    public MaskedIdentityResolver()
    {
    }

    public MaskedIdentityResolver(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public string? Resolve(Customer source, CustomerDto destination, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.EncryptedIdentityNumber))
            return null;

        if (_encryptionService != null)
        {
            var decrypted = _encryptionService.Decrypt(source.EncryptedIdentityNumber);
            return _encryptionService.Mask(decrypted);
        }

        return "********";
    }
}
