using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;
using Products.Application.Shared.Permissions;
using System.Security.Claims;

namespace Products.Application.Infrastructure.Persistence.Seeders
{
    public static class RolesSeeder
    {
        public static async Task CreateRoles(RoleManager<ApplicationRole> roleManager)
        {
            if (roleManager.Roles.Any())
                return;

            var adminRole = new ApplicationRole
            {
                Id = UserConstants.AdminRoleId,
                Name = "Admin",
                Description = "Admin Role",
                Active = true
            };

            await roleManager.CreateAsync(adminRole);

            foreach (var permission in Permissions.Factory.CreatePermissionsForModule())
            {
                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permission, permission));
            }
        }
    }
}
