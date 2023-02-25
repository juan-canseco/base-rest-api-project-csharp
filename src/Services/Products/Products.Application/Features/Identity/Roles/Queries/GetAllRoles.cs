using Application.Shared.Extensions;
using Application.Shared.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Products.Application.Domain;
using System.Linq.Expressions;

namespace Products.Application.Features.Identity.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<PagedList<GetAllRolesResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderBy { get; set; } = default!;
        public string? SortOrder { get; set; } = default!;
        public string? Filter { get; set; } = default!;
    }

    public class GetAllRolesHandler : IRequestHandler<GetAllRolesQuery, PagedList<GetAllRolesResponse>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public GetAllRolesHandler(RoleManager<ApplicationRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        private Expression<Func<ApplicationRole, object>> GetOrderByField(string? sortBy)
        {
            if (sortBy == "id")
            {
                return r => r.Id;
            }
            return r => r.Name;
        }

        public async Task<PagedList<GetAllRolesResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var orderBy = GetOrderByField(request.OrderBy);
            var filter = string.IsNullOrEmpty(request.Filter) ? "" : request.Filter;
            var sortOrder = request.SortOrder == "desc" ? request.SortOrder : "asc";
            var baseQuery = _roleManager.Roles.Where(r => EF.Functions.Like(r.Name, $"%{filter}%"));
            var finalQuery = sortOrder == "asc" ? baseQuery.OrderBy(orderBy) : baseQuery.OrderByDescending(orderBy);
            var roles =  await finalQuery.AsNoTracking().ToPagedListAsync(request.PageNumber, request.PageSize);
            var result = _mapper.Map<PagedList<GetAllRolesResponse>>(roles);
            return result;
        }
    }
    public class GetAllRolesResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!; 
        public bool Active { get; set; }
    }

    public class GetAllRolesMapProfile : Profile
    {
        public GetAllRolesMapProfile() 
        {
            CreateMap<ApplicationRole, GetAllRolesResponse>();
            CreateMap<PagedList<ApplicationRole>, PagedList<GetAllRolesResponse>>();
        }
    }
}
