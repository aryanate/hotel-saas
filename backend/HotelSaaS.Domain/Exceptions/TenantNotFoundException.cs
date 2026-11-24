namespace HotelSaaS.Domain.Exceptions;

public class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(Guid tenantId)
        : base($"Tenant with ID '{tenantId}' was not found.", "TENANT_NOT_FOUND")
    {
    }

    public TenantNotFoundException(string message, string? code = "TENANT_NOT_FOUND")
        : base(message, code)
    {
    }
}
