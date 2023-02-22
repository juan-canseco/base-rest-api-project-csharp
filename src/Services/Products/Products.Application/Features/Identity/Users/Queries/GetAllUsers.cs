using Application.Shared.Extensions;
using Application.Shared.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Products.Application.Domain;
using System.Linq.Expressions;

namespace Products.Application.Features.Identity.Users.Queries
{
    public class GetAllUsersQuery : IRequest<PagedList<GetAllUsersResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderBy { get; set; } = default!;
        public string SortOrder { get; set; } = default!;
        public string Filter { get; set; } = default!;
    }

    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        public GetAllUsersQueryValidator()
        {
            RuleFor(r => r)
              .Must(r => MustBeGreaterThanZero(r.PageNumber))
              .WithMessage("'PageNumber' must be greater than 0");

            RuleFor(r => r)
                .Must(r => MustBeGreaterThanZero(r.PageSize))
                .WithMessage("'PageSize' must be greater than 0.");
        }
        private bool MustBeGreaterThanZero(int value) => value > 0;
    }

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, PagedList<GetAllUsersResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public GetAllUsersHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        // Find an easiest way to do this
        private Expression<Func<ApplicationUser, object>> GetOrderByField(string sortBy)
        {
            var sortField = sortBy.ToLower();
            if (sortField == "fullname")
            {
                return r => r.Fullname;
            }
            if (sortField == "role")
            {
                return r => r.RoleId;
            }
            if (sortField == "active")
            {
                return r => r.Active;
            }
            if (sortField == "createdat")
            {
                return r => EF.Property<DateTime>(r, "CreatedAt");
            }
            if (sortField == "updatedat")
            {
                return r => EF.Property<DateTime>(r, "UpdatedAt");
            }
            if (sortField == "createdby")
            {
                return r => EF.Property<string>(r, "CreatedBy");
            }
            if (sortField == "updatedby")
            {
                return r => EF.Property<string>(r, "UpdatedBy");
            }
            return r => r.Fullname;
        }

        public async Task<PagedList<GetAllUsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var orderBy = GetOrderByField(request.OrderBy);
            var filter = string.IsNullOrEmpty(request.Filter) ? "" : request.Filter;
            var sortOrder = request.SortOrder == "desc" ? request.SortOrder : "asc";

            IQueryable<GetAllUsersResponse> query = from user in _userManager.Users
                                                        join role in _roleManager.Roles
                                                        on user.RoleId equals role.Id
                                                        where EF.Functions.Like(user.Fullname, $"%{filter}%")
                                                        orderby orderBy ascending
                                                        select new GetAllUsersResponse
                                                        {
                                                            Id = user.Id,
                                                            Fullname = user.Fullname,
                                                            Role = role.Name,
                                                            Active = user.Active
                                                        };

            if (request.SortOrder == "desc")
            {
                query = from user in _userManager.Users
                            join role in _roleManager.Roles
                            on user.RoleId equals role.Id
                            where EF.Functions.Like(user.Fullname, $"%{filter}%")
                            orderby orderBy descending
                            select new GetAllUsersResponse
                            {
                                Id = user.Id,
                                Fullname = user.Fullname,
                                Role = role.Name,
                                Active = user.Active
                            };

            }

            return await query.AsNoTracking().ToPagedListAsync(request.PageNumber, request.PageSize);

        }
    }

    public class GetAllUsersResponse
    {
        public string Id { get; set; } = default!;
        public string Fullname { get; set; } = default!;

        public string Role { get; set; } = default!;
        public bool Active { get; set; } = default!;
    }

}


