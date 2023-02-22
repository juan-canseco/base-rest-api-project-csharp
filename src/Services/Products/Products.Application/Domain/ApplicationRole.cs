using Microsoft.AspNetCore.Identity;

namespace Products.Application.Domain
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; } = default!;
        public bool Active { get; set; } = true;
    }
}
