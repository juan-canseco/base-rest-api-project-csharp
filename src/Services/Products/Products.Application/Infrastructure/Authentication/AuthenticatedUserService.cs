using Microsoft.AspNetCore.Http;
using Products.Application.Interfaces.Authentication;
using System.Security.Claims;

namespace Products.Application.Infrastructure.Authentication
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {

        private readonly IHttpContextAccessor _contextAccessor;

        public AuthenticatedUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public string UserId => _contextAccessor.HttpContext.User.FindFirstValue("uid");
    }
}
