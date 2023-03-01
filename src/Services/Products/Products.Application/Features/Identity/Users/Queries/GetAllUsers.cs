using Application.Shared.Models;
using Dapper;
using FluentValidation;
using MediatR;
using Products.Application.Interfaces.Persistence;

namespace Products.Application.Features.Identity.Users.Queries
{
    // https://www.davepaquette.com/archive/2019/01/28/paging-large-result-sets-with-dapper-and-sql-server.aspx
    public class GetAllUsersQuery : IRequest<PagedList<GetAllUsersResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderBy { get; set; } = default!;
        public string? SortOrder { get; set; } = default!;
        public string? Filter { get; set; } = default!;
    }

    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        public GetAllUsersQueryValidator() 
        {
            RuleFor(r => r.OrderBy)
                .Must(r => MustBeNullOrExistsInSet(r))
                .WithMessage("'OrderBy' invalid field.");

            RuleFor(r => r.PageNumber)
                .GreaterThan(0);

            RuleFor(r => r.PageSize)
                .GreaterThan(0);
        }

        public bool MustBeNullOrExistsInSet(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return true;
            }
            var set = new HashSet<string>
            {
                "id",
                "fullname",
                "role",
                "active"
            };
            return set.Contains(sortBy.ToLower());
        }
    }

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, PagedList<GetAllUsersResponse>>
    {
        private readonly IDapperContext _context;

        public GetAllUsersHandler(IDapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private string GetOrderByField(string? sortBy)
        {
            var sortFields = new Dictionary<string, string>
            {
                { "id", "u.Id" },
                { "fullname", "u.Fullname" },
                { "active", "u.Active" },
                { "role", "r.Name" }
            };
            return sortFields[sortBy ?? "fullname"];
        }

        public async Task<PagedList<GetAllUsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var orderBy = GetOrderByField(request.OrderBy);
            var filter = string.IsNullOrEmpty(request.Filter) ? "" : request.Filter;
            var sortOrder = request.SortOrder == "desc" ? "DESC" : "ASC";

            var query = $@"SELECT u.Id, u.Fullname, r.Name AS Role, u.Active 
                           FROM [Identity].[Users] u INNER JOIN [Identity].[Roles] r
                           ON u.RoleId = r.Id
                           WHERE u.Fullname LIKE CONCAT('%', @Filter, '%')
                           ORDER BY {orderBy} {sortOrder}
                           OFFSET @Offset ROWS
                           FETCH NEXT @PageSize ROWS ONLY;
                           
                           SELECT COUNT(*) FROM [Identity].[Users] u
                           INNER JOIN [Identity].[Roles] r
                           ON u.RoleId = r.Id
                           WHERE u.Fullname LIKE CONCAT('%', @Filter, '%');";

            var @params = new
            {
                PageSize = request.PageSize,
                Offset = (request.PageNumber - 1) * request.PageSize,
                Filter = filter
            };

            using (var connection = _context.CreateConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(query, @params))
                {
                    var items = multi.Read<GetAllUsersResponse>().ToList();
                    var count = multi.ReadFirst<int>();
                    return new PagedList<GetAllUsersResponse>(items, count, request.PageNumber, request.PageSize);
                }
            }
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


