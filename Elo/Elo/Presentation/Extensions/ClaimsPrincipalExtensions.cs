using System.Security.Claims;

namespace Elo.Presentation.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetEmpresaId(this ClaimsPrincipal user)
    {
        if (user == null)
        {
            return null;
        }

        var claim = user.FindFirst("companyId");
        return claim != null && int.TryParse(claim.Value, out var empresaId) ? empresaId : null;
    }

    public static bool IsGlobalAdmin(this ClaimsPrincipal user)
    {
        if (user == null)
        {
            return false;
        }

        return user.IsInRole("Admin") && !user.GetEmpresaId().HasValue;
    }
}
