using Application.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Products.Application.Domain;
using Products.Application.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utils.Time;

namespace Products.Application.Features.Identity.Auth.Commands
{
    public class GetTokenHandler : IRequestHandler<GetTokenCommand, GetTokenResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly JWTSettings _settings;

        public GetTokenHandler(UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signingInManager,
            IDateTimeProvider dateTimeProvider, 
            IOptions<JWTSettings> settings)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _signInManager = signingInManager ?? throw new ArgumentNullException(nameof(_signInManager));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<GetTokenResponse> Handle(GetTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new IdentityException($"No Accounts Registered with {request.Email}.");
            }

            var result =
                await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false,
                    lockoutOnFailure: false);

            if (!user.Active)
            {
                throw new IdentityException(
                    $"Account for '{request.Email}' is not active. Please contact the Administrator.");
            }

            if (!result.Succeeded)
            {
                throw new IdentityException($"Invalid Credentials for '{request.Email}'.");
            }

            var jwtSecurityToken = await GenerateJWToken(user);
            var role = await _roleManager.FindByIdAsync(user.RoleId);
            var rolePermissions = await _roleManager.GetClaimsAsync(role);
            var permissions = (from r in rolePermissions select r.Value).ToList();

            var securityToken = await GenerateJWToken(user);


            return new GetTokenResponse
            {
                UserId = user.Id,
                Role = role.Name,
                Email = user.Email,
                Fullname = user.Fullname,
                Permissions = permissions,
                IsVerified = user.EmailConfirmed,
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken)
            };

        }


        private async Task<JwtSecurityToken> GenerateJWToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            
            var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("uid", user.Id),
                    new Claim("full_name", user.Fullname),
                    new Claim("role_id", user.RoleId)
                }
                .Union(userClaims);

            return JWTGeneration(claims);
        }



        private JwtSecurityToken JWTGeneration(IEnumerable<Claim> claims)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: _dateTimeProvider.NowUtc(),
                expires: _dateTimeProvider.NowUtc().AddMinutes(_settings.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

    }

    public class GetTokenCommand : IRequest<GetTokenResponse>
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class GetTokenCommandValidator : AbstractValidator<GetTokenCommand>
    {
        public GetTokenCommandValidator() 
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(50);

            RuleFor(c => c.Password)
                .NotEmpty()
                .MaximumLength(30)
                .MinimumLength(6);
        }
    }

    public class GetTokenResponse
    {
        public string UserId { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public List<string> Permissions { get; set; } = default!;
        public bool IsVerified { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}
