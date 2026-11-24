using HotelSaaS.Domain.Interfaces;
using HotelSaaS.Infrastructure.Data;
using HotelSaaS.Infrastructure.Data.Repositories;
using HotelSaaS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Database Context
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString);
            }
        });
        services.AddScoped<HotelSaaS.Application.Common.Interfaces.IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        // Domain & Application Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IWhatsAppService, WhatsAppMockService>();
        services.AddScoped<IEmailService, EmailMockService>();
        services.AddScoped<JwtService>();
        services.AddScoped<HotelSaaS.Domain.Interfaces.ITokenService>(sp => sp.GetRequiredService<JwtService>());
        services.AddScoped<HotelSaaS.Application.Common.Interfaces.ITokenService>(sp => sp.GetRequiredService<JwtService>());

        return services;
    }
}
