using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointDefinition.Tests.TestHelpers;

/// <summary>
/// Test endpoint definition that throws an exception during service registration.
/// This is in a separate namespace to avoid being discovered during normal assembly scanning.
/// </summary>
internal class ThrowsOnDefineServicesEndpointDefinition : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        throw new InvalidOperationException("Test exception in DefineServices");
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
    }
}

/// <summary>
/// Test endpoint definition that throws an exception during endpoint registration.
/// This is in a separate namespace to avoid being discovered during normal assembly scanning.
/// </summary>
internal class ThrowsOnDefineEndpointsEndpointDefinition : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        throw new InvalidOperationException("Test exception in DefineEndpoints");
    }
}
