using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Products.Application.Constants;
using Products.Application.Domain;
using System.Security.Claims;

namespace Products.Application.Requirements
{
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public PermissionRequirementHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }
            var userId = context.User?.FindFirstValue("uid");
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _roleManager.FindByIdAsync(user.RoleId);

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var permissions = roleClaims
                .Where(x => x.Type == CustomClaimTypes.Permission && x.Value == requirement.Permission)
                .Select(x => x.Value);

            if (permissions.Any())
            {
                context.Succeed(requirement);
            }

        }
    }
}
