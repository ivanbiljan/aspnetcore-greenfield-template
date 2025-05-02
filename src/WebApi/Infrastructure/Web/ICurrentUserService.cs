using Microsoft.AspNetCore.Identity;

namespace WebApi.Infrastructure.Web;

internal interface ICurrentUserService
{
    string UserId { get; }
}

[RegisterScoped]
internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
    : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    
    public string UserId
    {
        get
        {
            return _userManager.GetUserId(_httpContextAccessor.HttpContext!.User) ??
                   throw new InvalidOperationException();
        }
    }
}