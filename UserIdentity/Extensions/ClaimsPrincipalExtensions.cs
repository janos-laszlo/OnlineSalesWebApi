using System.Security.Claims;

namespace UserIdentity.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public int UserId =>
            int.Parse(principal.Claims.
                First(c => c.Type == ClaimTypes.NameIdentifier)
                .Value);
    }
}