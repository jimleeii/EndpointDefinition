using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointDefinition.Tests;

/// <summary>
/// Test implementation of IEndpointDefinition for testing purposes
/// </summary>
public class TestEndpointDefinition : IEndpointDefinition
{
    public bool DefineServicesCalled { get; private set; }
    public bool DefineEndpointsCalled { get; private set; }

    public void DefineServices(IServiceCollection services)
    {
        DefineServicesCalled = true;
        services.AddSingleton<TestService>();
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        DefineEndpointsCalled = true;
        app.MapGet("/test", () => "test");
    }
}

/// <summary>
/// Another test implementation for testing multiple endpoint definitions
/// </summary>
public class AnotherTestEndpointDefinition : IEndpointDefinition
{
    public bool DefineServicesCalled { get; private set; }
    public bool DefineEndpointsCalled { get; private set; }

    public void DefineServices(IServiceCollection services)
    {
        DefineServicesCalled = true;
        services.AddScoped<AnotherTestService>();
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        DefineEndpointsCalled = true;
        app.MapGet("/another", () => "another");
    }
}

/// <summary>
/// Test service for dependency injection testing
/// </summary>
public class TestService
{
    public string GetMessage() => "Test Service";
}

/// <summary>
/// Another test service for testing multiple services
/// </summary>
public class AnotherTestService
{
    public string GetMessage() => "Another Test Service";
}
