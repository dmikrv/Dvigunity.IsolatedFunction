using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dvigunity.IsolatedFunction.Authentication.Schemes.JwtBearer;

public static class JwtBearerExtensions
{
    public static IsolatedFunctionAuthenticationBuilder AddJwtBearer(
        this IsolatedFunctionAuthenticationBuilder builder,
        IConfiguration configuration,
        string configSectionName = "AzureAd",
        string authenticationScheme = "Bearer")
    {
        var configurationSection = configuration.GetSection(configSectionName);
        var options = configurationSection.Get<JwtBearerOptions>();
        
        var ioptions = Options.Create(options);
        builder.Services.AddSingleton(ioptions);
        
        return builder;
    }
}