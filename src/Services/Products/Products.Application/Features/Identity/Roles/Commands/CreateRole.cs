using Application.Shared.Exceptions;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Constants;
using Products.Application.Domain;
using System.Security.Claims;

namespace Products.Application.Features.Identity.Roles.Commands
{
    public record CreateRoleCommand(string Name, string Description, List<string> Permissions) : IRequest<CreateRoleResponse>;

    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .NotNull()
                .WithMessage("'Name' is required.");

            RuleFor(r => r.Description)
                .NotEmpty()
                .NotNull()
                .WithMessage("'Description' is required.");

            RuleFor(r => r.Permissions)
                .NotNull()
                .NotEmpty()
                .WithMessage("'Permissions' is required.");

            RuleFor(r => r.Permissions)
                .Must(r => PermissionsValidator.Validate(r))
                .WithMessage("Invalid 'Permissions'.");
        }
    }

    public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public CreateRoleHandler(RoleManager<ApplicationRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var newRole = _mapper.Map<ApplicationRole>(request);

            var roleWithSameName = await _roleManager.FindByNameAsync(request.Name);

            if (roleWithSameName != null)
            {
                throw new IdentityException($"The role with the Name {request.Name} already exists.");
            }

            await _roleManager.CreateAsync(newRole);

            foreach (var permission in request.Permissions)
            {
                await _roleManager.AddClaimAsync(newRole, new Claim(CustomClaimTypes.Permission, permission));
            }
            var response = _mapper.Map<CreateRoleResponse>(newRole);
            response.Permissions = request.Permissions;
            return response;
        }
    }

    public class CreateRoleResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool Active { get; set; }
        public List<string> Permissions { get; set; } = default!;
    }

    public class CreateRoleMapProfile : Profile
    {
        public CreateRoleMapProfile()
        {
            CreateMap<CreateRoleCommand, ApplicationRole>();
            CreateMap<ApplicationRole, CreateRoleResponse>();
        }
    }

}
