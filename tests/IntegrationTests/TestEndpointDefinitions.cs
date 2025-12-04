using EndpointDefinition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegrationTests;

/// <summary>
/// Test endpoint definition for weather API
/// </summary>
public class WeatherEndpoints : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton<IWeatherService, WeatherService>();
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        app.MapGet("/weather", (IWeatherService weatherService) => 
        {
            return weatherService.GetWeather();
        });

        app.MapGet("/weather/{city}", (string city, IWeatherService weatherService) => 
        {
            return weatherService.GetWeatherForCity(city);
        });
    }
}

/// <summary>
/// Test endpoint definition for user API
/// </summary>
public class UserEndpoints : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        app.MapGet("/users", (IUserService userService) => 
        {
            return userService.GetUsers();
        });

        app.MapGet("/users/{id:int}", (int id, IUserService userService) => 
        {
            var user = userService.GetUser(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        });

        app.MapPost("/users", (User user, IUserService userService) => 
        {
            userService.AddUser(user);
            return Results.Created($"/users/{user.Id}", user);
        });
    }
}

/// <summary>
/// Environment-aware endpoint definition
/// </summary>
public class DebugEndpoints : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        // No services needed
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.MapGet("/debug/info", () => new 
            { 
                Environment = env.EnvironmentName,
                ApplicationName = env.ApplicationName
            });
        }
    }
}

/// <summary>
/// Endpoint definition demonstrating constructor injection
/// </summary>
public class LoggingEndpoints : IEndpointDefinition
{
    private readonly ILogger<LoggingEndpoints> _logger;

    public LoggingEndpoints(ILogger<LoggingEndpoints> logger)
    {
        _logger = logger;
    }

    public void DefineServices(IServiceCollection services)
    {
        // No additional services needed
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        app.MapGet("/logging/test", () => 
        {
            _logger.LogInformation("Test endpoint was called");
            return new { Message = "Logger is working", LoggerType = _logger.GetType().Name };
        });
    }
}

// Supporting types and services
public interface IWeatherService
{
    string GetWeather();
    string GetWeatherForCity(string city);
}

public class WeatherService : IWeatherService
{
    public string GetWeather() => "Sunny";
    public string GetWeatherForCity(string city) => $"Weather in {city}: Cloudy";
}

public interface IUserService
{
    List<User> GetUsers();
    User? GetUser(int id);
    void AddUser(User user);
}

public class UserService : IUserService
{
    private static readonly List<User> _users = new()
    {
        new User { Id = 1, Name = "John Doe" },
        new User { Id = 2, Name = "Jane Smith" }
    };

    public List<User> GetUsers() => _users;
    public User? GetUser(int id) => _users.FirstOrDefault(u => u.Id == id);
    public void AddUser(User user) => _users.Add(user);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
