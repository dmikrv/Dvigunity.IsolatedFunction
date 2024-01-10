namespace Dvigunity.IsolatedFunction.Authentication.Schemes.JwtBearer;

public class JwtBearerOptions
{
    public const string SchemeName = "Bearer";
    public string Authority { get; set; } = null!;
    public string ClientId { get; set; } = null!;
}