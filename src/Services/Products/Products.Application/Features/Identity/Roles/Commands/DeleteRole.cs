using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Roles.Commands
{
    public record DeleteRoleCommand(string RoleId) : IRequest<Unit>;

    public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        public DeleteRoleCommandValidator()
        {
            RuleFor(x => x.RoleId)
                 .NotNull()
                 .NotEmpty()
                 .WithMessage("'RoleId' is required.");
        }
    }

    public class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, Unit>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteRoleHandler(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with the Id : '{request.RoleId}' was not found.");
            }

            if (await _userManager.Users.Where(u => u.RoleId.Equals(request.RoleId)).AnyAsync())
            {
                throw new IdentityException($"The role with the Id {request.RoleId} it's in use and cannot be deleted.");
            }
            await _roleManager.DeleteAsync(role);

            return Unit.Value;
        }
    }
}
