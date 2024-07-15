using Dvigunity.IsolatedFunction.FuncContextAccessor;
using Dvigunity.IsolatedFunction.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Dvigunity.IsolatedFunction.Extensions;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder UseIsolatedFunctionAuthentication(this IFunctionsWorkerApplicationBuilder worker)
    {
        return worker.UseWhen<AuthenticationMiddleware>(ctx =>
        {
            if (ctx.FunctionDefinition.Name.Contains("Swagger"))
                return false;

            var isHttpTrigger = ctx.FunctionDefinition.InputBindings.Values.Any(a => a.Type == "httpTrigger");
            if (!isHttpTrigger)
                return false;

            return true;
        });
    }

    public static IFunctionsWorkerApplicationBuilder UseIsolatedFunctionAuthorization(this IFunctionsWorkerApplicationBuilder worker)
    {
        return worker.UseWhen<AuthorizationMiddleware>(ctx =>
        {
            if (ctx.FunctionDefinition.Name.Contains("Swagger"))
                return false;

            var isHttpTrigger = ctx.FunctionDefinition.InputBindings.Values.Any(a => a.Type == "httpTrigger");
            if (!isHttpTrigger)
                return false;

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