using Application.Shared.Exceptions;

namespace Products.Application.Shared.Permissions
{
    public class Permissions
    {
        public class Dashboard
        {
            public const string View = "Permissions.Dashboard.View";

            public static IReadOnlyCollection<string> All = new List<string>()
            {
                View
            }.AsReadOnly();
        }

        public class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";

            public static IReadOnlyCollection<string> All => new List<string>()
            {
                View,
                Create,
                Edit,
                Delete
            }
            .AsReadOnly();
        }

        public class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";

            public static IReadOnlyCollection<string> All => new List<string>()
            {
                View,
                Create,
                Edit,
                Delete
            }.AsReadOnly();

        }

        public class Products
        {
            public const string View = "Permissions.Products.View";
            public const string Create = "Permissions.Products.Create";
            public const string Edit = "Permissions.Products.Edit";
            public const string Delete = "Permissions.Products.Delete";

            public static IReadOnlyCollection<string> All => new List<string>()
            {
                View,
                Create,
                Edit,
                Delete
            }
            .AsReadOnly();
        }

        public class Factory
        {

            private static IReadOnlyCollection<string> GetAllPermissions()
            {
                var permissions = new List<string>();
                permissions.AddRange(Users.All);
                permissions.AddRange(Roles.All);
                permissions.AddRange(Products.All);
                permissions.AddRange(Dashboard.All);
                return permissions.AsReadOnly();
            }

            public static IReadOnlyCollection<string> CreatePermissionsForModule(string module = Modules.All)
            {
                switch (module)
                {
                    case Modules.Dashboard:
                        return Dashboard.All;
                    case Modules.Users:
                        return Users.All;
                    case Modules.Roles:
                        return Users.All;
                    case Modules.Products:
                        return Products.All;
                    case Modules.All:
                        return GetAllPermissions();
                    default:
                        throw new DomainException($"The module {module} was not found.");
                }
            }
        }

    }
}
