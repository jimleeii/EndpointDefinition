namespace EndpointDefinition;

/// <summary>
/// The endpoint definition extensions.
/// </summary>
public static class EndpointDefinitionExtensions
{
    /// <summary>
    /// Add endpoint definitions.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="scanMarkers">The scan markers.</param>
    public static void AddEndpointDefinitions(this IServiceCollection services, params Type[] scanMarkers)
    {
        if (scanMarkers == null || scanMarkers.Length == 0)
        {
            throw new ArgumentNullException(nameof(scanMarkers), "Scan markers cannot be null or empty.");
        }

        var endpointDefinitionTypes = scanMarkers
            .SelectMany(marker => marker.Assembly.ExportedTypes
                .Where(type => typeof(IEndpointDefinition).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract))
                .ToList();

        if (endpointDefinitionTypes.Count == 0)
        {
            // No endpoint definitions found, no need to proceed.
            return;
        }

        foreach (var type in endpointDefinitionTypes)
        {
            services.AddTransient(type);
        }

        var serviceProvider = services.BuildServiceProvider();
        var endpointDefinitions = endpointDefinitionTypes
            .Select(type =>
            {
                try
                {
                    return (IEndpointDefinition)serviceProvider.GetRequiredService(type);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to create an instance of {type.FullName}", ex);
                }
            })
            .ToList();

        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.DefineServices(services);
        }

        services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IEndpointDefinition>);
    }

    /// <summary>
    /// Use endpoint definitions.
    /// </summary>
    /// <param name="app">The app.</param>
    /// <param name="evnt">The environment.</param>
    public static void UseEndpointDefinitions(this WebApplication app, IWebHostEnvironment evnt)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(evnt);

        if (app.Services.GetService(typeof(IReadOnlyCollection<IEndpointDefinition>)) is not IReadOnlyCollection<IEndpointDefinition> definitions)
        {
            return;
        }

        Parallel.ForEach(definitions, endpointDefinition =>
        {
            try
            {
                endpointDefinition.DefineEndpoints(app, evnt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to define endpoints for {endpointDefinition.GetType().FullName}", ex);
            }
        });
    }
}