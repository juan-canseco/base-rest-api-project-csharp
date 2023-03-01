namespace Products.Application.Shared.Permissions
{
    public class Modules
    {
        public const string Dashboard = "Dashboard";
        public const string Products = "Products";
        public const string Users = "Users";
        public const string Roles = "Roles";
        public const string All = "All";

        public static IReadOnlyList<string> GetAll() => new List<string>()
        {
            Dashboard,
            Products,
            Users,
            Roles
        }.AsReadOnly();
    }
}
