using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using HotelSaaS.API.Middleware;
using HotelSaaS.Application;
using HotelSaaS.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting HotelSaaS API application...");

    // Add Services from Application and Infrastructure layers
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    // Rate Limiting (AspNetCoreRateLimit)
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // Response Compression
    builder.Services.AddResponseCompression();

    // Health Checks
    builder.Services.AddHealthChecks();

    // Controllers with camelCase and ignore null values
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // JWT Authentication
    var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "CHANGE_ME_TO_A_VERY_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HotelSaaS";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HotelSaaSClients";

    builder.Services.AddAuthentication(options =>
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
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    // Authorization policies: "HotelAdmin", "Receptionist", "Accountant", "SuperAdmin"
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SuperAdmin", policy => policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("SuperAdmin") || ctx.User.HasClaim("is_super_admin", "true")));

        options.AddPolicy("HotelAdmin", policy => policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("HotelAdmin") || ctx.User.IsInRole("SuperAdmin") || ctx.User.HasClaim("is_super_admin", "true")));

        options.AddPolicy("Receptionist", policy => policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Receptionist") || ctx.User.IsInRole("HotelAdmin") || ctx.User.IsInRole("SuperAdmin") || ctx.User.HasClaim("is_super_admin", "true")));

        options.AddPolicy("Accountant", policy => policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Accountant") || ctx.User.IsInRole("HotelAdmin") || ctx.User.IsInRole("SuperAdmin") || ctx.User.HasClaim("is_super_admin", "true")));
    });

    // Swagger / OpenAPI with JWT Bearer
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "HotelSaaS API",
            Version = "v1",
            Description = "Hotel Management SaaS RESTful API"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Middleware Pipeline
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelSaaS API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseIpRateLimiting();
    app.UseResponseCompression();
    app.UseHttpsRedirection();
    var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    if (!Directory.Exists(wwwrootPath))
    {
        Directory.CreateDirectory(wwwrootPath);
    }
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("CorsPolicy");

    app.UseAuthentication();
    app.UseMiddleware<TenantMiddleware>();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Automatic Database Initialization in Development
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelSaaS.Infrastructure.Data.ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            HotelSaaS.Infrastructure.Data.DatabaseSeeder.SeedAllAsync(dbContext).GetAwaiter().GetResult();
            Log.Information("Database initialization & seed check completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database initialization warning: Could not ensure database was created automatically.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
