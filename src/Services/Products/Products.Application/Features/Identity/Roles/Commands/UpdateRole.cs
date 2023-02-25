using Application.Shared.Exceptions;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;
using Products.Application.Shared.Permissions;
using System.Security.Claims;

namespace Products.Application.Features.Identity.Roles.Commands
{
    public record UpdateRoleCommand(string RoleId, string Name, string Description, List<string> Permissions) : IRequest<UpdateRoleResponse>;
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(r => r.Name)
              .NotEmpty();

            RuleFor(r => r.Description)
                .NotEmpty();

            RuleFor(r => r.Permissions)
                .NotNull()
                .NotEmpty();

            RuleFor(r => r.Permissions)
                .Must(r => PermissionsValidator.Validate(r))
                .WithMessage("Invalid 'Permissions'.");
        }
    }

    public class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, UpdateRoleResponse>
    {

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public UpdateRoleHandler(RoleManager<ApplicationRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<UpdateRoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with the Id : '{request.RoleId}' was not found.");
            }

            if (!role.Active)
            {
                throw new IdentityException($"Role with the Id : '{request.RoleId}' it's disabled.");
            }

            if (!role.Name.Equals(request.Name))
            {
                var roleWithTheSameName = await _roleManager.FindByNameAsync(request.Name);

                if (roleWithTheSameName != null)
                {
                    throw new IdentityException($"The role with the Name {request.Name} already exists.");
                }
            }


            var oldClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in oldClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            
            foreach (var permission in request.Permissions)
            {
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, permission));
            }

            role.Name = request.Name;
            role.Description = request.Description;
            await _roleManager.UpdateAsync(role);
            
            var response = _mapper.Map<UpdateRoleResponse>(role);
            response.Permissions = request.Permissions;

            return response;
        }
    }

    public class UpdateRoleResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool Active { get; set; }

        public List<string> Permissions { get; set; } = default!;
    }

    public class UpdateRoleMapProfile : Profile
    {
        public UpdateRoleMapProfile()
        {
            CreateMap<ApplicationRole, UpdateRoleResponse>();
        }
    }

}
