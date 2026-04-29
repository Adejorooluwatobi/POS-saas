using Microsoft.Extensions.DependencyInjection;
using POS.Domain.Interfaces;
using POS.Domain.Repositories;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Services;
using POS.Infrastructure.Data.Interceptors;

namespace POS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<AuditInterceptor>();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenGenerator>();
        services.AddSingleton<IReceiptNumberService, ReceiptNumberService>();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<ITillSessionRepository, TillSessionRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITerminalRepository, TerminalRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<IGiftCardRepository, GiftCardRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
