namespace EndpointDefinition;

/// <summary>
/// Extension methods for registering and using endpoint definitions.
/// </summary>
public static class EndpointDefinitionExtensions
{
    /// <summary>
    /// Adds endpoint definitions to the service collection by scanning assemblies containing the specified marker types.
    /// </summary>
    /// <param name="services">The service collection to add endpoint definitions to.</param>
    /// <param name="scanMarkers">One or more types whose assemblies will be scanned for <see cref="IEndpointDefinition"/> implementations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scanMarkers"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an endpoint definition cannot be instantiated.</exception>
    /// <remarks>
    /// Endpoint definitions can use constructor injection to receive dependencies. Dependencies will be resolved
    /// from a temporary service provider built from the current service collection state.
    /// </remarks>
    public static void AddEndpointDefinitions(this IServiceCollection services, params Type[] scanMarkers)
    {
        if (scanMarkers == null || scanMarkers.Length == 0)
        {
            throw new ArgumentNullException(nameof(scanMarkers), "Scan markers cannot be null or empty.");
        }

        var endpointDefinitionTypes = scanMarkers
            .SelectMany(marker => marker.Assembly.ExportedTypes
                .Where(type => typeof(IEndpointDefinition).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract))
            .Distinct()
            .ToList();

        if (endpointDefinitionTypes.Count == 0)
        {
            // No endpoint definitions found, no need to proceed.
            return;
        }

        // Build a temporary service provider to support constructor injection for endpoint definitions
        using var serviceProvider = services.BuildServiceProvider();

        // Instantiate endpoint definitions using ActivatorUtilities to support constructor injection
        var endpointDefinitions = new List<IEndpointDefinition>(endpointDefinitionTypes.Count);

        foreach (var type in endpointDefinitionTypes)
        {
            try
            {
                // Use ActivatorUtilities to support both parameterless constructors and constructor injection
                var instance = (IEndpointDefinition)ActivatorUtilities.CreateInstance(serviceProvider, type);
                endpointDefinitions.Add(instance);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create an instance of {type.FullName}. Ensure the type has a public constructor and all dependencies are registered.", ex);
            }
        }

        // Call DefineServices on all endpoint definitions
        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.DefineServices(services);
        }

        // Register the collection of endpoint definitions as a singleton for later use
        services.AddSingleton<IReadOnlyCollection<IEndpointDefinition>>(endpointDefinitions);
    }

    /// <summary>
    /// Configures endpoints by calling <see cref="IEndpointDefinition.DefineEndpoints"/> on all registered endpoint definitions.
    /// </summary>
    /// <param name="app">The web application to configure endpoints for.</param>
    /// <param name="env">The hosting environment.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or <paramref name="env"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an endpoint definition fails to define endpoints.</exception>
    public static void UseEndpointDefinitions(this WebApplication app, IWebHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(env);

        if (app.Services.GetService(typeof(IReadOnlyCollection<IEndpointDefinition>)) is not IReadOnlyCollection<IEndpointDefinition> definitions)
        {
            return;
        }

        // Register endpoints sequentially to ensure thread safety
        // ASP.NET Core endpoint registration methods (MapGet, MapPost, etc.) are not thread-safe
        foreach (var endpointDefinition in definitions)
        {
            try
            {
                endpointDefinition.DefineEndpoints(app, env);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to define endpoints for {endpointDefinition.GetType().FullName}.", ex);
            }
        }
    }
}