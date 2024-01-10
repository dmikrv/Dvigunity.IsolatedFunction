namespace Dvigunity.IsolatedFunction.Authorization;

public class AuthorizationOptions
{
    public HashSet<string> RequiredScopes { get; set; } = null!;
}