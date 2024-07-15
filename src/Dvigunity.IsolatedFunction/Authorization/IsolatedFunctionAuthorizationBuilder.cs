using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dvigunity.IsolatedFunction.Authorization;

/// <summary>
///     Used to configure authentication
/// </summary>
public class IsolatedFunctionAuthorizationBuilder
{
    /// <summary>
    ///     Initializes a new instance of <see cref="IsolatedFunctionAuthorizationBuilder" />.
    /// </summary>
    /// <param name="services">The services being configured.</param>
    /// <param name="authorizationOptions"></param>
    public IsolatedFunctionAuthorizationBuilder(IServiceCollection services, IOptions<AuthorizationOptions> authorizationOptions)
    {
        Services = services;
        AuthorizationOptions = authorizationOptions;
    }
    
    /// <summary>
    ///     The services being configured.
    /// </summary>
    public virtual IServiceCollection Services { get; }
    
    protected virtual IOptions<AuthorizationOptions> AuthorizationOptions { get; }
    
    public IsolatedFunctionAuthorizationBuilder AddRequiredScope(params string[] scopes)
    {
        foreach (var scope in scopes)
        {
            AuthorizationOptions.Value.RequiredScopes.Add(scope);
        }
        
        return this;
    }
}