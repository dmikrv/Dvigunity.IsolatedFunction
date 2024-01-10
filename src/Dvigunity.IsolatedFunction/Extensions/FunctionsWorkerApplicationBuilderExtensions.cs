using Dvigunity.IsolatedFunction.FuncContextAccessor;
using Dvigunity.IsolatedFunction.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Dvigunity.IsolatedFunction.Extensions;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder UseIsolatedFunctionAuthentication(this IFunctionsWorkerApplicationBuilder worker,
        bool bypassNoHttp = true, bool bypassSwagger = true)
    {
        return worker.UseWhen<AuthenticationMiddleware>(ctx =>
        {
            if (bypassSwagger && ctx.FunctionDefinition.Name.Contains("Swagger"))
                return false;

            if (bypassNoHttp)
                // use this middleware only for http trigger invocations
                return ctx.FunctionDefinition.InputBindings.Values
                    .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";

            return true;
        });
    }

    public static IFunctionsWorkerApplicationBuilder UseIsolatedFunctionAuthorization(this IFunctionsWorkerApplicationBuilder worker,
        bool bypassNoHttp = true, bool bypassSwagger = true)
    {
        return worker.UseWhen<AuthorizationMiddleware>(ctx =>
        {
            if (bypassSwagger && ctx.FunctionDefinition.Name.Contains("Swagger"))
                return false;

            if (bypassNoHttp)
                // use this middleware only for http trigger invocations
                return ctx.FunctionDefinition.InputBindings.Values
                    .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";

            return true;
        });
    }

    public static IFunctionsWorkerApplicationBuilder UseIsolatedFunctionExceptionHandler(this IFunctionsWorkerApplicationBuilder worker)
    {
        return worker.UseMiddleware<IsolatedFunctionExceptionHandlingMiddleware>();
    }

    public static IFunctionsWorkerApplicationBuilder UseFunctionContextAccessor(this IFunctionsWorkerApplicationBuilder worker)
    {
        return worker.UseMiddleware<FunctionContextAccessorMiddleware>();
    }
}