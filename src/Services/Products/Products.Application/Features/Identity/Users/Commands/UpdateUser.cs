using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Users.Commands
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UpdateUserHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            var newRole = await _roleManager.FindByIdAsync(request.RoleId);

            if (user == null)
            {
                throw new NotFoundException($"The User with the Id {request.UserId} was not found.");
            }

            if (newRole == null)
            {
                throw new NotFoundException($"The Role with the Id {request.RoleId} was not found.");
            }

            bool roleChanged = request.RoleId != user.RoleId;
            string previousRoleId = user.RoleId;

            if (roleChanged)
            {
                var prevRole = await _roleManager.FindByIdAsync(previousRoleId);
                await _userManager.RemoveFromRoleAsync(user, prevRole.Name);
                await _userManager.AddToRoleAsync(user, newRole.Name);
            }

            user.Fullname = request.Fullname;
            user.RoleId = request.RoleId;

            await _userManager.UpdateAsync(user);

            var rolePermissions = await _roleManager.GetClaimsAsync(newRole);

            var permissions = (from r in rolePermissions select r.Value).ToList();

            var response = new UpdateUserResponse
            {
                Id = user.Id,
                Fullname = user.Fullname,
                Active = user.Active,
                Role = new UpdateUserRoleResponse
                {
                    Id = newRole.Id,
                    Name = newRole.Name,
                    Permissions = permissions
                }
            };
            return response;
        }
    }

    public class UpdateUserCommand : IRequest<UpdateUserResponse>
    {
        public string UserId { get; set; } = default!;
        public string RoleId { get; set; } = default!;
        public string Fullname { get; set; } = default!;
    }

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(r => r.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("'UserId' is required.");

            RuleFor(r => r.RoleId)
                .NotNull()
                .NotEmpty()
                .WithMessage("'RoleId' is required.");


            RuleFor(r => r.Fullname)
                .NotNull()
                .NotEmpty()
                .WithMessage("'Fullname' is required.");

            RuleFor(r => r.Fullname)
                .MinimumLength(2)
                .WithMessage("The length of the Fullname is too short. The minimum length must be 2 characters.");

            RuleFor(r => r.Fullname)
                .MaximumLength(50)
                .WithMessage("The length of the Fullname is too long. The maximum length must be 50 characters.");
        }
    }

    public class UpdateUserRoleResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<string> Permissions { get; set; } = default!;
    }

    public class UpdateUserResponse
    {
        public string Id { get; set; } = default!;
        public UpdateUserRoleResponse Role { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool Active { get; set; }
    }
}
