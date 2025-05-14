using System.Security.Claims;

public interface IRequestContext
{
    string? GetCurrentUserId();
    ClaimsPrincipal User { get; }
    bool IsUserAuthenticated { get; }
}

