using EndpointDefinition;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configure the test environment
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseEnvironment("Development");
            webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
        });

        return base.CreateHost(builder);
    }
}

/// <summary>
/// Test program marker class
/// </summary>
public partial class TestProgram
{
}
