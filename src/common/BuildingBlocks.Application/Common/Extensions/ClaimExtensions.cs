using System.Security.Claims;
using BuildingBlocks.SharedKernel;

namespace BuildingBlocks.Application.Common.Extensions;

public static partial class ClaimsPrincipalExtensions
{
    public static Option<string> GetClaim(this IEnumerable<Claim> claims, string claimType,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        => claims
            .GetClaims(claimType, comparison)
            .Select(c => c.Value)
            .FirstOrDefault();

    public static IEnumerable<Claim> GetClaims(this IEnumerable<Claim> claims, string claimType,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        => claims
            .Where(c => c.Type.Equals(claimType, comparison));


    public static Option<string> GetUserId(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal
            .Claims
            .GetClaim(ClaimTypes.NameIdentifier);
}