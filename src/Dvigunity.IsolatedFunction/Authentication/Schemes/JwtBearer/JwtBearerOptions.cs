namespace Dvigunity.IsolatedFunction.Authentication.Schemes.JwtBearer;

public class JwtBearerOptions
{
    public const string SchemeName = "Bearer";
    public string? Authority { get; set; }
    public string? Instance { get; set; }
    public string? TenantId { get; set; }
    public string ClientId { get; set; } = null!;
}