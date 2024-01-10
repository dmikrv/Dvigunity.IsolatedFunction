using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dvigunity.IsolatedFunction.Authentication;

/// <summary>
///     Used to configure authentication
/// </summary>
public class IsolatedFunctionAuthenticationBuilder
{
    /// <summary>
    ///     Initializes a new instance of <see cref="IsolatedFunctionAuthenticationBuilder" />.
    /// </summary>
    /// <param name="services">The services being configured.</param>
    /// <param name="authenticationOptions"></param>
    public IsolatedFunctionAuthenticationBuilder(IServiceCollection services, IOptions<AuthenticationOptions> authenticationOptions)
    {
        Services = services;
        AuthenticationOptions = authenticationOptions;
    }

    /// <summary>
    ///     The services being configured.
    /// </summary>
    public virtual IServiceCollection Services { get; }

    protected virtual IOptions<AuthenticationOptions> AuthenticationOptions { get; }
}