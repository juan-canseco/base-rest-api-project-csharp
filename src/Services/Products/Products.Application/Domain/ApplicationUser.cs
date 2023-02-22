using Microsoft.AspNetCore.Identity;

namespace Products.Application.Domain
{
    public class ApplicationUser : IdentityUser 
    {
        public string RoleId { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool Active { get; set; }
    }
}
