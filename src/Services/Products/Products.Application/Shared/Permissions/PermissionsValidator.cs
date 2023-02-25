namespace Products.Application.Shared.Permissions
{
    public class PermissionsValidator
    {
        public static bool Validate(IReadOnlyCollection<string> permissions)
        {
            var permissionsSet = Permissions.Factory.CreatePermissionsForModule(Modules.All);
            if (permissions.Count > permissionsSet.Count)
            {
                return false;
            }
            foreach (var permission in permissions)
            {
                if (!permissionsSet.Contains(permission))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
