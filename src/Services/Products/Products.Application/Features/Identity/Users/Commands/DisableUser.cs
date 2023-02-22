using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Users.Commands
{
    public class DisableUserHandler : IRequestHandler<DisableUserCommand, DisableUserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DisableUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<DisableUserResponse> Handle(DisableUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"The User with the Id {request.UserId} was not found.");
            }

            if (!user.Active)
            {
                throw new IdentityException($"The User with the Id {request.UserId} it's alredy disabled.");
            }

            user.Active = false;
            await _userManager.UpdateAsync(user);

            return new DisableUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Fullname = user.Fullname,
                Active = user.Active
            };
        }
    }

    public class DisableUserCommand : IRequest<DisableUserResponse>
    {
        public string UserId { get; set; } = default!;
    }

    public class DisableUserCommandValidator : AbstractValidator<DisableUserCommand>
    {
        public DisableUserCommandValidator()
        {
            RuleFor(r => r.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("'UserId' is required.");
        }
    }

    public class DisableUserResponse
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool Active { get; set; } = default!;
    }
}
