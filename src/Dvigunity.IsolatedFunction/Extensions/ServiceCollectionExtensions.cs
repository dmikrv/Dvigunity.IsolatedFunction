using Dvigunity.IsolatedFunction.Authentication;
using Dvigunity.IsolatedFunction.Authorization;
using Dvigunity.IsolatedFunction.FuncContextAccessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dvigunity.IsolatedFunction.Extensions;

public static class ServiceCollectionExtensions
{
    public static IsolatedFunctionAuthenticationBuilder AddIsolatedFunctionAuthentication(
        this IServiceCollection services,
        string defaultScheme = "Bearer")
    {
        var ioptions = Options.Create(new AuthenticationOptions
        {
            DefaultScheme = defaultScheme
        });
        services.AddSingleton(ioptions);

        return new IsolatedFunctionAuthenticationBuilder(services, ioptions);
    }

    public static IsolatedFunctionAuthorizationBuilder AddIsolatedFunctionAuthorization(this IServiceCollection services)
    {
        var options = new AuthorizationOptions
        {
            RequiredScopes = new HashSet<string>()
        };

        var ioptions = Options.Create(options);
        services.AddSingleton(ioptions);

        return new IsolatedFunctionAuthorizationBuilder(services, Options.Create(options));
    }

    public static IServiceCollection AddFunctionContextAccessor(this IServiceCollection services)
    {
        // The accessor itself should be registered as a singleton, but the context
        // within the accessor will be scoped to the Function invocation
        return services.AddSingleton<IFunctionContextAccessor, FunctionContextAccessor>();
    }
}