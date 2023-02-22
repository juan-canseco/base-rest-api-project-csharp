using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Roles.Commands
{
    public record EnableRoleCommand(string RoleId) : IRequest<Unit>;

    public class EnableRoleCommandValidator : AbstractValidator<EnableRoleCommand>
    {
        public EnableRoleCommandValidator()
        {
            RuleFor(x => x.RoleId)
                .NotNull()
                .NotEmpty()
                .WithMessage("'RoleId' is required.");
        }
    }

    public class EnableRoleHandler : IRequestHandler<EnableRoleCommand, Unit>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        public EnableRoleHandler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Unit> Handle(EnableRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with the Id : '{request.RoleId}' was not found.");
            }

            if (role.Active)
                throw new IdentityException($"Role with the Id : '{request.RoleId}' it's already enabled.");

            role.Active = true;

            await _roleManager.UpdateAsync(role);

            return Unit.Value;
        }
    }
}
