using Products.Application.Domain;

namespace Products.Application.Interfaces.Authentication
{
    public class TokenSettings
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public double DurationInMinutes { get; set; }
    }

    public class TokenResponse<TPaylod> where TPaylod: class
    {
        public string Token { get; set; } = default!;
        public TPaylod Payload { get; set; } = default!;
        public DateTime IssuedOn { get; set; } = default!;
        public DateTime ExpiresOn { get; set; } = default!;
    }

    public interface ITokenService<TPayload> where TPayload : class
    {
        Task<TokenResponse<TPayload>> GetTokenAsync(ApplicationUser user);
    }
}
