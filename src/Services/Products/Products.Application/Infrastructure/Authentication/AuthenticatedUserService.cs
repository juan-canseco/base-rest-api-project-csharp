using Microsoft.AspNetCore.Http;
using Products.Application.Interfaces.Authentication;
using System.Security.Claims;

namespace Products.Application.Infrastructure.Authentication
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor contextAccessor)
        {
            UserId = contextAccessor?.HttpContext?.User.FindFirstValue("uid");
        }

        public string? UserId { get; } = default!;
    }
}
