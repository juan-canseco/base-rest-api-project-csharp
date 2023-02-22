using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;
using Products.Application.Extensions;

namespace Products.Application.Features.Identity.Users.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public CreateUserHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                Fullname = request.Fullname,
                RoleId = request.RoleId,
                Active = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                throw new IdentityException($"Email {request.Email} is already registered.");
            }

            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new IdentityException($"The Role with the Id {request.RoleId} does not exists.");
            }

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errorsDictionary = result
                    .Errors
                    .ToDictionary();
                throw new IdentityException(errorsDictionary);
            }

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }


            var rolePermissions = await _roleManager.GetClaimsAsync(role);

            var permissions = (from r in rolePermissions select r.Value).ToList();

            var response = new CreateUserResponse
            {
                Id = user.Id,
                Fullname = user.Fullname,
                Active = user.Active,
                Role = new CreateUserRoleResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Permissions = permissions
                }
            };

            return response;
        }
    }

    public class CreateUserCommand : IRequest<CreateUserResponse>
    {
        public string RoleId { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public string Email { get; set; } =default!;
        public string Password { get; set; } = default!;
    }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator() 
        {
            RuleFor(r => r.RoleId)
            .NotNull()
            .NotEmpty()
            .WithMessage("'RoleId' is required.");

            RuleFor(r => r.Email)
                .NotNull()
                .NotEmpty()
                .WithMessage("'Email' is required.");

            RuleFor(r => r.Email)
                .EmailAddress()
                .WithMessage("'Email' invalid format.");

            RuleFor(r => r.Email)
                .MaximumLength(50)
                .WithMessage("The length of the Email is too long. The maximum length must be 50 characters.");

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

            RuleFor(r => r.Password)
                .NotNull()
                .NotEmpty()
                .WithMessage("'Password' is required.");

            RuleFor(r => r.Password)
                .MinimumLength(6)
                .WithMessage("The length of the LastName is too short. The minimum length must be 6 characters.");

            RuleFor(r => r.Password)
                .MaximumLength(30)
                .WithMessage("The length of the LastName is too long. The maximum length must be 30 characters.");
        }
    }


    public class CreateUserRoleResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<string> Permissions { get; set; } = default!;
    }

    public class CreateUserResponse
    {
        public string Id { get; set; } = default!;
        public CreateUserRoleResponse Role { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool Active { get; set; } 
    }
}
