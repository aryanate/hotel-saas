namespace HotelSaaS.Domain.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user making the request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the hotel (tenant) ID of the current user. Null for super admins.</summary>
    Guid? HotelId { get; }

    /// <summary>Gets the unique identifier of the current user.</summary>
    Guid UserId { get; }

    /// <summary>Gets the email address of the current user.</summary>
    string Email { get; }

    /// <summary>Gets the roles assigned to the current user.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Gets a value indicating whether the current user is a super administrator.</summary>
    bool IsSuperAdmin { get; }
}
