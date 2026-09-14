using System.Text;
using Identity.Application.Common.Interfaces;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Data.Interceptors;
using Identity.Infrastructure.Identity;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddDatabase(configuration)
                .AddCaching()
                .AddServices(configuration)
                .AddJwtAuthentication(configuration)
                .AddJwtAuthorization();

            return services;
        }


        private static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddSingleton(TimeProvider.System);

            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

            services.AddScoped<IAppDbContext>(provider =>
                provider.GetRequiredService<AppDbContext>());

            return services;
        }

        private static IServiceCollection AddCaching(
        this IServiceCollection services)
        {
            services.AddHybridCache();
            return services;
        }

        private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var jwtSettings = configuration
                .GetSection(JwtSettings.SectionName)
                .Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    $"Configuration section '{JwtSettings.SectionName}' is missing.");

            services
                .AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .ValidateOnStart();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudiences = jwtSettings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

        private static IServiceCollection AddJwtAuthorization(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Authenticated",
                    policy => policy.RequireAuthenticatedUser());

                options.AddPolicy("AdminOnly",
                    policy => policy.RequireRole("Admin"));

                options.AddPolicy("RestaurantOwnerOnly",
                    policy => policy.RequireRole("RestaurantOwner"));

                options.AddPolicy("CustomerOnly",
                    policy => policy.RequireRole("Customer"));
            });

            return services;
        }
        private static IServiceCollection AddServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<ITokenProvider, TokenProviderService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddHttpContextAccessor();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(30);
            });

            var emailSettings = configuration
                .GetSection(EmailSettings.SectionName)
                .Get<EmailSettings>()
                ?? throw new InvalidOperationException(
                    $"Configuration section '{EmailSettings.SectionName}' is missing.");

            services
                .AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName)
                .AddSmtpSender(
                    emailSettings.SmtpServer,
                    emailSettings.SmtpPort,
                    emailSettings.Username,
                    emailSettings.Password);

            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
