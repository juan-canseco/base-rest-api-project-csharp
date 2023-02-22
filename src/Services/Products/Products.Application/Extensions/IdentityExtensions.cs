using Microsoft.AspNetCore.Identity;

namespace Products.Application.Extensions
{
    public static class IdentityExtensions
    {
        public static IReadOnlyDictionary<string, string[]> ToDictionary(this IEnumerable<IdentityError> errors)
        {
            return errors
                .Select(err => err)
                .GroupBy(
                    err => err.Code,
                    err => err.Description,
                    (code, message) => new
                    {
                        Key = code,
                        Values = message.Distinct().ToArray()
                    }).ToDictionary(x => x.Key, x => x.Values);
        }
    }
}
