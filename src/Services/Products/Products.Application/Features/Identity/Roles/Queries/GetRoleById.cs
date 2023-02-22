using Application.Shared.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Products.Application.Domain;

namespace Products.Application.Features.Identity.Roles.Queries
{
    public record GetRoleByIdQuery(string RoleId) : IRequest<GetRoleByIdResponse>;

    public class GetRoleByIdHandler : IRequestHandler<GetRoleByIdQuery, GetRoleByIdResponse>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public GetRoleByIdHandler(RoleManager<ApplicationRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<GetRoleByIdResponse> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with the Id : '{request.RoleId}' was not found.");
            }
            var result = _mapper.Map<GetRoleByIdResponse>(role);
            result.Permissions = await GetRolePermissionsAsync(role);
            return result;
        }

        private async Task<List<string>> GetRolePermissionsAsync(ApplicationRole role)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            return (from c in claims select c.Value).ToList();
        }
    }

    public class GetRoleByIdResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<string> Permissions { get; set; } = default!;

        public bool Active { get; set; }
    }

    public class GetRoleByIdMapProfile : Profile
    {
        public GetRoleByIdMapProfile()
        {
            CreateMap<ApplicationRole, GetRoleByIdResponse>();
        }
    }

}
