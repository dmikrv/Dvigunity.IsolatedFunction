using System.Reflection;
using System.Security.Claims;
using Dvigunity.IsolatedFunction.Authorization;
using Dvigunity.IsolatedFunction.Exceptions;
using Dvigunity.IsolatedFunction.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dvigunity.IsolatedFunction.Middleware;

public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private const string ScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";
    private readonly IOptions<AuthorizationOptions> _authorizationOptions;
    private readonly ILogger<AuthorizationMiddleware> _logger;
    
    public AuthorizationMiddleware(
        IOptions<AuthorizationOptions> authorizationOptions,
        ILogger<AuthorizationMiddleware> logger)
    {
        _authorizationOptions = authorizationOptions;
        _logger = logger;
    }
    
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var claimsPrincipal = context.Features.Get<ClaimsPrincipal>();
        
        if (claimsPrincipal is null)
        {
            _logger.LogInformation("No claims principal found, skipping authorization");
            await next(context);
            return;
        }
        
        // if (!CheckRequiredScope(claimsPrincipal))
        //     throw new AuthorizationException($"No required scope in '{ScopeClaimType}' claim",
        //         "No required scope");
        
        if (!AuthorizePrincipal(context, claimsPrincipal, _authorizationOptions.Value))
        {
            throw new AuthorizationException("Forbidden", "Forbidden");
        }
        
        await next(context);
    }
    
    private bool CheckRequiredScope(ClaimsPrincipal principal)
    {
        if (!principal.HasClaim(c => c.Type == ScopeClaimType))
        {
            return false;
        }
        
        // Scopes are stored in a single claim, space-separated
        var callerScopes = (principal.FindFirst(ScopeClaimType)?.Value ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var requiredScopes = _authorizationOptions.Value.RequiredScopes;
        
        return requiredScopes.All(scope => callerScopes.Contains(scope));
    }
    
    private static bool AuthorizePrincipal(FunctionContext context, ClaimsPrincipal principal,
        AuthorizationOptions authorizationOptions)
    {
        // This authorization implementation was made
        // for Azure AD. Your identity provider might differ.
        
        if (principal.HasClaim(c => c.Type == ScopeClaimType))
        {
            // Check required scopes
            var callerScopes = (principal.FindFirst(ScopeClaimType)?.Value ?? "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var requiredScopes = authorizationOptions.RequiredScopes;
            if (!requiredScopes.All(scope => callerScopes.Contains(scope)))
            {
                return false;
            }
            
            // Request made with delegated permissions, check scopes and user roles
            return AuthorizeDelegatedPermissions(context, principal);
        }
        
        // Request made with application permissions, check app roles
        return AuthorizeApplicationPermissions(context, principal);
    }
    
    private static bool AuthorizeDelegatedPermissions(FunctionContext context, ClaimsPrincipal principal)
    {
        var targetMethod = context.GetTargetFunctionMethod();
        
        var (acceptedScopes, acceptedUserRoles) = GetAcceptedScopesAndUserRoles(targetMethod);
        
        var userHasAcceptedRole = true;
        if (acceptedScopes.Any())
        {
            var userRoles = principal.FindAll(ClaimTypes.Role);
            userHasAcceptedRole = userRoles.Any(ur => acceptedUserRoles.Contains(ur.Value));
        }
        
        var callerHasAcceptedScope = true;
        if (acceptedUserRoles.Any())
        {
            // Scopes are stored in a single claim, space-separated
            var callerScopes = (principal.FindFirst(ScopeClaimType)?.Value ?? "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            callerHasAcceptedScope = callerScopes.Any(cs => acceptedScopes.Contains(cs));
        }
        
        // This app requires both a scope and user role
        // when called with scopes, so we check both
        return userHasAcceptedRole && callerHasAcceptedScope;
    }
    
    private static bool AuthorizeApplicationPermissions(FunctionContext context, ClaimsPrincipal principal)
    {
        var targetMethod = context.GetTargetFunctionMethod();
        
        var acceptedAppRoles = GetAcceptedAppRoles(targetMethod);
        var appRoles = principal.FindAll(ClaimTypes.Role);
        var appHasAcceptedRole = appRoles.Any(ur => acceptedAppRoles.Contains(ur.Value));
        return appHasAcceptedRole;
    }
    
    private static (List<string> scopes, List<string> userRoles) GetAcceptedScopesAndUserRoles(MethodInfo targetMethod)
    {
        var attributes = GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(targetMethod);
        // If scopes A and B are allowed at class level,
        // and scope A is allowed at method level,
        // then only scope A can be allowed.
        // This finds those common scopes and
        // user roles on the attributes.
        var scopes = attributes
            .Skip(1)
            .Select(a => a.Scopes)
            .Aggregate(attributes.FirstOrDefault()?.Scopes ?? Enumerable.Empty<string>(),
                (result, acceptedScopes) => { return result.Intersect(acceptedScopes); })
            .ToList();
        var userRoles = attributes
            .Skip(1)
            .Select(a => a.UserRoles)
            .Aggregate(attributes.FirstOrDefault()?.UserRoles ?? Enumerable.Empty<string>(),
                (result, acceptedRoles) => { return result.Intersect(acceptedRoles); })
            .ToList();
        return (scopes, userRoles);
    }
    
    private static List<string> GetAcceptedAppRoles(MethodInfo targetMethod)
    {
        var attributes = GetCustomAttributesOnClassAndMethod<AuthorizeAttribute>(targetMethod);
        // Same as above for scopes and user roles,
        // only allow app roles that are common in
        // class and method level attributes.
        return attributes
            .Skip(1)
            .Select(a => a.AppRoles)
            .Aggregate(attributes.FirstOrDefault()?.UserRoles ?? Enumerable.Empty<string>(),
                (result, acceptedRoles) => { return result.Intersect(acceptedRoles); })
            .ToList();
    }
    
    private static List<T> GetCustomAttributesOnClassAndMethod<T>(MethodInfo targetMethod)
        where T : Attribute
    {
        var methodAttributes = targetMethod.GetCustomAttributes<T>();
        var classAttributes = targetMethod.DeclaringType!.GetCustomAttributes<T>();
        return methodAttributes.Concat(classAttributes).ToList();
    }
}