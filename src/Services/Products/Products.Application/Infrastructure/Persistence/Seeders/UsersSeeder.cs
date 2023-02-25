using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;
using Products.Application.Shared.Permissions;

namespace Products.Application.Infrastructure.Persistence.Seeders
{
    public static class UsersSeeder
    {
        public static async Task CreateUsers(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            if (userManager.Users.Any())
                return;

            var user = new ApplicationUser
            {
                RoleId = UserConstants.AdminRoleId,
                Email = UserConstants.AdminEmail,
                UserName = UserConstants.AdminEmail,
                Fullname = "Admin",
                EmailConfirmed = true,
                Active = true
            };
            await userManager.CreateAsync(user, UserConstants.AdminPassword);
            var role = await roleManager.FindByIdAsync(UserConstants.AdminRoleId);
            await userManager.AddToRoleAsync(user, role.Name);

        }
    }
}
