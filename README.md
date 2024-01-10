[![CI](https://github.com/dmikrv/Dvigunity.IsolatedFunction/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/dmikrv/Dvigunity.IsolatedFunction/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/dmikrv/Dvigunity.IsolatedFunction/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Kravchuk.IsolatedFunction?label=NuGet)](https://www.nuget.org/packages/Kravchuk.IsolatedFunction)

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