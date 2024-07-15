using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json;
using Dvigunity.IsolatedFunction.Authentication;
using Dvigunity.IsolatedFunction.Authentication.Schemes.JwtBearer;
using Dvigunity.IsolatedFunction.Exceptions;
using Dvigunity.IsolatedFunction.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Dvigunity.IsolatedFunction.Middleware;

public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtSecurityTokenHandler _tokenValidator;
    
    public AuthenticationMiddleware(IOptions<JwtBearerOptions> jwtBearerOptions,
        ILogger<AuthenticationMiddleware> logger)
    {
        _logger = logger;
        
        var authority = jwtBearerOptions.Value.Authority
                        ?? $"{jwtBearerOptions.Value.Instance}/{jwtBearerOptions.Value.TenantId}/v2.0";
        var audience = jwtBearerOptions.Value.ClientId;
        
        _tokenValidator = new();
        _tokenValidationParameters = new()
        {
            ValidAudience = audience
        };
        _configurationManager = new(
            $"{authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
    }
    
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var targetMethod = context.GetTargetFunctionMethod();
        
        if (IsMethodAllowAnonymous(targetMethod))
        {
            _logger.LogInformation("Method is marked as AllowAnonymous");
            await next(context);
            return;
        }
        
        if (!TryGetTokenFromHeaders(context, out var token))
        {
            throw new AuthenticationException("Unable to get bearer token from headers.", "No token in headers");
        }
        
        if (!_tokenValidator.CanReadToken(token))
        {
            throw new AuthenticationException("Token is malformed.", "Token is not valid");
        }
        
        // Get OpenID Connect metadata
        var validationParameters = _tokenValidationParameters.Clone();
        var openIdConfig = await _configurationManager.GetConfigurationAsync(default);
        validationParameters.ValidIssuer = openIdConfig.Issuer;
        validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;
        
        try
        {
            // Validate token
            var principal = _tokenValidator.ValidateToken(token, validationParameters, out _);
            
            // Set principal in Features collection
            // It can be accessed from here later in the call chain
            context.Features.Set(principal);
        }
        catch (ArgumentException)
        {
            throw new AuthenticationException("Token is malformed.", "Token is not valid");
        }
        catch (SecurityTokenExpiredException)
        {
            throw new AuthenticationException("Bearer token is expired.", "Token is expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            throw new AuthenticationException("Token's signature is not properly formatted.", "Token is not valid");
        }
        
        await next(context);
    }
    
    private static bool TryGetTokenFromHeaders(FunctionContext context, out string? token)
    {
        token = null;
        // HTTP headers are in the binding context as a JSON object
        // The first checks ensure that we have the JSON string
        if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
        {
            return false;
        }
        
        if (headersObj is not string headersStr)
        {
            return false;
        }
        
        // Deserialize headers from JSON
        var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersStr)
                      ?? throw new ArgumentNullException(nameof(headersStr));
        var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
        if (!normalizedKeyHeaders.TryGetValue("authorization", out var authHeaderValue))
            // No Authorization header present
        {
            return false;
        }
        
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) // Scheme is not Bearer
        {
            return false;
        }
        
        token = authHeaderValue["Bearer ".Length..].Trim();
        return true;
    }
    
    private static bool IsMethodAllowAnonymous(MethodInfo targetMethod)
    {
        return GetCustomAttributesOnClassAndMethod<AllowAnonymousAttribute>(targetMethod).Any();
    }
    
    private static List<T> GetCustomAttributesOnClassAndMethod<T>(MethodInfo targetMethod)
        where T : Attribute
    {
        var methodAttributes = targetMethod.GetCustomAttributes<T>();
        var classAttributes = targetMethod.DeclaringType.GetCustomAttributes<T>();
        return methodAttributes.Concat(classAttributes).ToList();
    }
}