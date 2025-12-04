using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointDefinition.Tests.TestHelpers;

/// <summary>
/// Test endpoint definition without parameterless constructor.
/// This is in a separate namespace to avoid being discovered during assembly scanning.
/// </summary>
internal class NoParameterlessConstructorEndpointDefinition : IEndpointDefinition
{
    public NoParameterlessConstructorEndpointDefinition(string parameter)
    {
    }

    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
    }
}
