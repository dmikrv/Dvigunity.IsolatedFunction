# Dvigunity.IsolatedFunction

## Authentication & Authorization

```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseFunctionContextAccessor();
        worker.UseIsolatedFunctionExceptionHandler();
        worker.UseIsolatedFunctionAuthentication();
        worker.UseIsolatedFunctionAuthorization();
    }
    .ConfigureServices((context, services) =>
    {
        services.AddFunctionContextAccessor();
            
        services.AddIsolatedFunctionAuthentication()
            .AddJwtBearer(context.Configuration, "AzureAd");
        
        services.AddIsolatedFunctionAuthorization()
            .AddRequiredScope(context.Configuration.GetSection("AzureAd").GetValue<string>("RequiredApiScopeName"));
    }
```