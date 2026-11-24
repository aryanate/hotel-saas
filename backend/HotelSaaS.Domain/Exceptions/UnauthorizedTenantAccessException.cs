namespace HotelSaaS.Domain.Exceptions;

public class UnauthorizedTenantAccessException : DomainException
{
    public UnauthorizedTenantAccessException()
        : base("Access to this tenant's resources is not authorized.", "UNAUTHORIZED_TENANT_ACCESS")
    {
    }

    public UnauthorizedTenantAccessException(string message, string? code = "UNAUTHORIZED_TENANT_ACCESS")
        : base(message, code)
    {
    }
}
