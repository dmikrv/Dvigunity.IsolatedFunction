using System.Net;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dvigunity.IsolatedFunction.Extensions;

public static class FunctionContextExtensions
{
    public static async Task SetHttpResponseStatusCodeAsync(this FunctionContext context, HttpStatusCode statusCode)
    {
        var httpRequest = await context.GetHttpRequestDataAsync();
        var httpResponse = httpRequest!.CreateResponse(statusCode);
        SetHttpResponse(context, httpResponse);
    }

    public static async Task SetHttpResponseAsync<T>(this FunctionContext context, T writeData, HttpStatusCode statusCode)
    {
        var httpRequest = await context.GetHttpRequestDataAsync();
        var httpResponse = httpRequest!.CreateResponse();
        await httpResponse.WriteAsJsonAsync(writeData, statusCode);
        SetHttpResponse(context, httpResponse);
    }

    public static MethodInfo GetTargetFunctionMethod(this FunctionContext context)
    {
        // More terrible reflection code..
        // Would be nice if this was available out of the box on FunctionContext

        var entryPoint = context.FunctionDefinition.EntryPoint;

        var assemblyPath = context.FunctionDefinition.PathToAssembly;
        var assembly = Assembly.LoadFrom(assemblyPath);
        var typeName = entryPoint[..entryPoint.LastIndexOf('.')];
        var type = assembly.GetType(typeName);
        var methodName = entryPoint[(entryPoint.LastIndexOf('.') + 1)..];
        var method = type!.GetMethod(methodName)!;
        return method;
    }

    public static void SetHttpResponse(this FunctionContext context, HttpResponseData httpResponseData)
    {
        var invocationResult = context.GetInvocationResult();

        var httpOutputBindingFromMultipleOutputBindings = GetHttpOutputBindingFromMultipleOutputBinding(context);
        if (httpOutputBindingFromMultipleOutputBindings is not null)
            httpOutputBindingFromMultipleOutputBindings.Value = httpResponseData;
        else
            invocationResult.Value = httpResponseData;
    }

    private static OutputBindingData<HttpResponseData>? GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
    {
        // The output binding entry name will be "$return" only when the function return type is HttpResponseData
        var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

        return httpOutputBinding;
    }
}