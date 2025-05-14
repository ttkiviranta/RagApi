using RagApi.Data;
using System.Security.Claims;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _dbContext;

    public RequestContext(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

    public bool IsUserAuthenticated => User?.Identity?.IsAuthenticated == true;

    public string? GetCurrentUserId()
    {
        return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

