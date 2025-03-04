using System.Security.Claims;

namespace DemoLab7.Auth;

public class KeycloakRoleClaimsTransformation : Microsoft.AspNetCore.Authentication.IClaimsTransformation
{
    private const string CLAIM_REALM_ACCESS = "realm_access";
    private const string CLAIM_ROLES = "roles";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity;
        var realmAccessClaim = principal.FindFirst(CLAIM_REALM_ACCESS);

        if (realmAccessClaim != null)
        {
            try
            {
                var realmAccess = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(realmAccessClaim.Value);
                if (realmAccess != null && realmAccess.ContainsKey(CLAIM_ROLES))
                {
                    var roles = realmAccess[CLAIM_ROLES];
                    if (roles != null && roles.Any())
                    {
                        Console.WriteLine("Roles Found:");
                        foreach (var role in roles)
                        {
                            var roleClaim = new Claim(ClaimTypes.Role,  role);
                            Console.WriteLine($"Claim Type: {roleClaim.Type}, Claim Value: {roleClaim.Value}");
                            Console.WriteLine("Adding role: " + roleClaim.Value); 
                            identity.AddClaim(roleClaim);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No roles found in realm_access.");
                    }
                }
                else
                {
                    Console.WriteLine("realm_access does not contain roles.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing realm_access claim: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("realm_access claim not found.");
        }

        return Task.FromResult(principal);
    }
   
}