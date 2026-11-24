using System.Net;
using System.Text.Json;
using HotelSaaS.API.Common;
using HotelSaaS.Application.Common.Exceptions;
using HotelSaaS.Domain.Exceptions;

namespace HotelSaaS.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled by the client (HTTP 499).");
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var errorCode = "INTERNAL_SERVER_ERROR";
        var message = "An unexpected error occurred. Please try again later.";
        IDictionary<string, string[]>? validationErrors = null;

        switch (exception)
        {
            case EntityNotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                errorCode = notFoundEx.Code ?? "NOT_FOUND";
                message = notFoundEx.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", message);
                break;

            case TenantNotFoundException tenantNotFoundEx:
                statusCode = HttpStatusCode.NotFound;
                errorCode = tenantNotFoundEx.Code ?? "TENANT_NOT_FOUND";
                message = tenantNotFoundEx.Message;
                _logger.LogWarning(exception, "Tenant not found: {Message}", message);
                break;

            case UnauthorizedTenantAccessException authTenantEx:
                statusCode = HttpStatusCode.Forbidden;
                errorCode = authTenantEx.Code ?? "FORBIDDEN_TENANT_ACCESS";
                message = authTenantEx.Message;
                _logger.LogWarning(exception, "Unauthorized tenant access: {Message}", message);
                break;

            case BusinessRuleViolationException bizRuleEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = bizRuleEx.Code ?? "BUSINESS_RULE_VIOLATION";
                message = bizRuleEx.Message;
                _logger.LogWarning(exception, "Business rule violation: {Message}", message);
                break;

            case DomainException domainEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = domainEx.Code ?? "DOMAIN_ERROR";
                message = domainEx.Message;
                _logger.LogWarning(exception, "Domain exception: {Message}", message);
                break;

            case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                message = valEx.Message;
                validationErrors = valEx.Errors;
                _logger.LogWarning(exception, "Validation exception: {Message}", message);
                break;

            case UnauthorizedAccessException authEx:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                message = string.IsNullOrWhiteSpace(authEx.Message) ? "Unauthorized access." : authEx.Message;
                _logger.LogWarning(exception, "Unauthorized access: {Message}", message);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                if (_env.IsDevelopment())
                {
                    message = exception.Message;
                }
                break;
        }

        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Message = message,
                Error = new ApiError
                {
                    Code = errorCode,
                    Message = message,
                    ValidationErrors = validationErrors
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}
