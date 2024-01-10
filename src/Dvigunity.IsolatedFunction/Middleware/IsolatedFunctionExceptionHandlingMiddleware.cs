using System.Net;
using Dvigunity.IsolatedFunction.Exceptions;
using Dvigunity.IsolatedFunction.Extensions;
using Dvigunity.IsolatedFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Dvigunity.IsolatedFunction.Middleware;

internal sealed class IsolatedFunctionExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (IsolatedFunctionException exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(FunctionContext context, IsolatedFunctionException exception)
    {
        var httpRequest = await context.GetHttpRequestDataAsync();
        if (httpRequest is null) return; // if it's not http trigger

        var httpResponse = httpRequest.CreateResponse();

        var problemDetails = new ProblemDetails
        {
            Status = (int)exception.StatusCode,
            Title = exception.Title ?? "An error has occured.",
            Detail = exception.Message
        };

        await httpResponse.WriteAsJsonAsync((object)problemDetails, (HttpStatusCode?)problemDetails.Status
                                                                    ?? HttpStatusCode.InternalServerError);
        context.SetHttpResponse(httpResponse);
    }
}