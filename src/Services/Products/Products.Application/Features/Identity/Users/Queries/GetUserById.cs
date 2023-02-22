using Application.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Users.Queries
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public GetUserByIdHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"The User with the Id {request.UserId} was not found.");
            }

            var role = await _roleManager.FindByIdAsync(user.RoleId);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var rolePermissions = (from c in roleClaims select c.Value).ToList();

            return new GetUserByIdResponse
            {
                Id = user.Id,
                Fullname = user.Fullname,
                Email = user.Email,
                Active = user.Active,
                Role = new GetUserByIdRoleResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Permissions = rolePermissions
                }
            };
        }
    }

    public class GetUserByIdQuery : IRequest<GetUserByIdResponse>
    {
        public string UserId { get; set; } = default!;
    }
    public class GetUserByIdRoleResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<string> Permissions { get; set; } = default!;
    }
    public class GetUserByIdResponse
    {
        public string Id { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool Active { get; set; }
        public GetUserByIdRoleResponse Role { get; set; } = default!;
    }

}
