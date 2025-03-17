# EndpointDefinition

[![NuGet](https://img.shields.io/nuget/v/EndpointDefinition.svg)](https://www.nuget.org/packages/EndpointDefinition/)
[![Build & Publish](https://github.com/jimleeii/EndpointDefinition/actions/workflows/publish.yml/badge.svg)](https://github.com/jimleeii/EndpointDefinition/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight library for organizing and registering API endpoints in ASP.NET Core applications using a clean, modular approach.

## Features

- **Modular Endpoint Organization**: Define endpoints in separate classes for improved maintainability
- **Dependency Injection Support**: Register services specific to each endpoint
- **Environment-aware Configuration**: Configure endpoints differently based on environment
- **Automatic Registration**: Easily scan and register all endpoint definitions in your assemblies
- **Parallel Endpoint Registration**: Improves startup performance for applications with many endpoints

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package EndpointDefinition
```

Or via the Package Manager Console:

```powershell
Install-Package EndpointDefinition
```

## Usage

### Step 1: Create Endpoint Definition Classes

Create classes that implement the `IEndpointDefinition` interface:

```csharp
using EndpointDefinition;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace YourNamespace.Endpoints
{
    public class WeatherEndpoints : IEndpointDefinition
    {
        public void DefineServices(IServiceCollection services)
        {
            // Register services required by this endpoint
            services.AddScoped<IWeatherService, WeatherService>();
        }

        public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
        {
            // Define endpoints
            app.MapGet("/weather", (IWeatherService weatherService) => 
            {
                return weatherService.GetForecast();
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();
            
            // Define different endpoints based on environment
            if (env.IsDevelopment())
            {
                app.MapGet("/weather/debug", () => "Debug endpoint");
            }
        }
    }
}
```

### Step 2: Register Endpoint Definitions in Program.cs

Register and use the endpoint definitions in your `Program.cs` file:

```csharp
using EndpointDefinition;
using YourNamespace.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointDefinitions(typeof(Program));
// Or specify multiple marker types:
// builder.Services.AddEndpointDefinitions(typeof(Program), typeof(WeatherEndpoints));

var app = builder.Build();

// Use the endpoint definitions
app.UseEndpointDefinitions(app.Environment);

app.Run();
```

## Advanced Usage

### Organizing Endpoints by Feature

Create separate endpoint definition classes for different features:

```csharp
public class UserEndpoints : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        var group = app.MapGroup("/users").WithTags("Users");
        
        group.MapGet("/", (IUserService userService) => userService.GetAllUsers());
        group.MapGet("/{id}", (int id, IUserService userService) => userService.GetUserById(id));
        group.MapPost("/", (UserCreateDto user, IUserService userService) => userService.CreateUser(user));
        // ...
    }
}
```

### Conditional Endpoint Registration

Register endpoints based on specific conditions:

```csharp
public class AdminEndpoints : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        // Only register these endpoints in non-production environments
        if (!env.IsProduction())
        {
            var group = app.MapGroup("/admin").WithTags("Admin").RequireAuthorization("AdminOnly");
            
            group.MapGet("/statistics", (IAdminService adminService) => adminService.GetStatistics());
            group.MapPost("/reset-data", (IAdminService adminService) => adminService.ResetData());
        }
    }
}
```

## Contributing

Contributions are welcome! Here's how you can contribute:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Submit a pull request

## Versioning

This project uses [SemVer](http://semver.org/) for versioning. The version is automatically updated in the CI/CD pipeline when changes are made to the project file.

## CI/CD

This project uses GitHub Actions for continuous integration and deployment. The workflow automatically:

- Builds and tests the project
- Packs the library into a NuGet package
- Publishes the package to NuGet.org when the version is updated
- Creates a GitHub release with the version tag

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.