using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Roles.Commands
{
    public record DisableRoleCommand(string RoleId) : IRequest<Unit>;

    public class DisableRoleCommandValidator : AbstractValidator<DisableRoleCommand>
    {
        public DisableRoleCommandValidator()
        {
            RuleFor(x => x.RoleId)
             .NotNull()
             .NotEmpty()
             .WithMessage("'RoleId' is required.");
        }
    }


    public class DisableRole : IRequestHandler<DisableRoleCommand, Unit>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DisableRole(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Unit> Handle(DisableRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with the Id : '{request.RoleId}' was not found.");
            }

            if (!role.Active)
                throw new IdentityException($"Role with the Id : '{request.RoleId}' it's already disabled.");

            role.Active = false;

            await _roleManager.UpdateAsync(role);
            return Unit.Value;
        }
    }
}
