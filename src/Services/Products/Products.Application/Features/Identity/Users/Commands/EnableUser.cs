
using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Users.Commands
{
    public class EnableUserHandler : IRequestHandler<EnableUserCommand, EnableUserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EnableUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<EnableUserResponse> Handle(EnableUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"The User with the Id {request.UserId} was not found.");
            }

            if (user.Active)
            {
                throw new IdentityException($"The User with the Id {request.UserId} it's alredy enabled.");
            }

            user.Active = true;
            await _userManager.UpdateAsync(user);

            return new EnableUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Fullname = user.Fullname,
                Active = user.Active
            };
        }
    }

    public class EnableUserCommand : IRequest<EnableUserResponse>
    {
        public string UserId { get; set; } = default!;
    }

    public class EnableUserCommandValidator : AbstractValidator<EnableUserCommand>
    {
        public EnableUserCommandValidator()
        {
            RuleFor(r => r.UserId)
                .NotNull()
                .NotEmpty()
                .WithMessage("'UserId' is required.");
        }
    }

    public class EnableUserResponse
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool Active { get; set; } = default!;
    }
}
