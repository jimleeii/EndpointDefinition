namespace EndpointDefinition;

/// <summary>
/// Interface for defining endpoints and services.
/// </summary>
public interface IEndpointDefinition
{
    /// <summary>
    /// Defines services required by the application.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    void DefineServices(IServiceCollection services);

    /// <summary>
    /// Defines endpoints for the web application.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="evnt">The hosting environment.</param>
    void DefineEndpoints(WebApplication app, IWebHostEnvironment evnt);
}
