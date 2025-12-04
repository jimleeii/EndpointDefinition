namespace EndpointDefinition;

/// <summary>
/// Defines a contract for organizing API endpoints and their dependencies in a modular way.
/// Implementations can group related endpoints and register their required services.
/// </summary>
public interface IEndpointDefinition
{
    /// <summary>
    /// Registers services required by the endpoints defined in this definition.
    /// This method is called during application startup before endpoint configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    void DefineServices(IServiceCollection services);

    /// <summary>
    /// Configures the endpoints for this definition. This method is called after
    /// the application has been built and middleware is being configured.
    /// </summary>
    /// <param name="app">The web application instance to configure endpoints on.</param>
    /// <param name="env">The hosting environment, allowing environment-specific endpoint configuration.</param>
    void DefineEndpoints(WebApplication app, IWebHostEnvironment env);
}
