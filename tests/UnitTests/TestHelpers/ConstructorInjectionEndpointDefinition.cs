using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EndpointDefinition.Tests;

/// <summary>
/// Test endpoint definition that uses constructor injection.
/// </summary>
public class ConstructorInjectionEndpointDefinition : IEndpointDefinition
{
    private readonly ILogger<ConstructorInjectionEndpointDefinition> _logger;

    public ConstructorInjectionEndpointDefinition(ILogger<ConstructorInjectionEndpointDefinition> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoggerInjected = true;
    }

    public bool LoggerInjected { get; }

    public bool DefineServicesCalled { get; private set; }

    public bool DefineEndpointsCalled { get; private set; }

    public void DefineServices(IServiceCollection services)
    {
        DefineServicesCalled = true;
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        DefineEndpointsCalled = true;
    }
}
