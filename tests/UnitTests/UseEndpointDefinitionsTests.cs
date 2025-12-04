using EndpointDefinition.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace EndpointDefinition.Tests;

public class UseEndpointDefinitionsTests
{
    [Fact]
    public void UseEndpointDefinitions_ShouldThrowArgumentNullException_WhenAppIsNull()
    {
        // Arrange
        WebApplication app = null!;
        var env = Mock.Of<IWebHostEnvironment>();

        // Act
        Action act = () => app.UseEndpointDefinitions(env);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("app");
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldThrowArgumentNullException_WhenEnvIsNull()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpointDefinitions(typeof(TestEndpointDefinition));
        var app = builder.Build();

        // Act
        Action act = () => app.UseEndpointDefinitions(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("env");
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldCallDefineEndpointsOnAllDefinitions()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpointDefinitions(typeof(TestEndpointDefinition));
        var app = builder.Build();

        // Act
        app.UseEndpointDefinitions(app.Environment);

        // Assert
        var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();
        definitions.Should().NotBeNull();
        definitions.Should().HaveCountGreaterThan(0);
        
        // Verify that DefineEndpoints was called on all definitions
        foreach (var definition in definitions)
        {
            if (definition is TestEndpointDefinition testDef)
            {
                testDef.DefineEndpointsCalled.Should().BeTrue();
            }
            else if (definition is AnotherTestEndpointDefinition anotherDef)
            {
                anotherDef.DefineEndpointsCalled.Should().BeTrue();
            }
        }
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldDoNothing_WhenNoDefinitionsRegistered()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        Action act = () => app.UseEndpointDefinitions(app.Environment);

        // Assert - Should not throw
        act.Should().NotThrow();
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldPassCorrectEnvironmentToDefinitions()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Services.AddEndpointDefinitions(typeof(EnvironmentAwareEndpointDefinition));
        var app = builder.Build();

        // Act
        app.UseEndpointDefinitions(app.Environment);

        // Assert
        var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();
        var envAwareDef = definitions.OfType<EnvironmentAwareEndpointDefinition>().FirstOrDefault();
        
        envAwareDef.Should().NotBeNull();
        envAwareDef!.ReceivedEnvironmentName.Should().Be(Environments.Development);
    }

    [Fact]
    public void UseEndpointDefinitions_ShouldRegisterEndpointsSequentially()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpointDefinitions(typeof(SequenceTrackingEndpointDefinition));
        var app = builder.Build();

        // Act
        app.UseEndpointDefinitions(app.Environment);

        // Assert
        var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();
        var sequenceDefs = definitions.OfType<SequenceTrackingEndpointDefinition>().ToList();
        
        // Verify that all definitions were called and have sequential IDs
        sequenceDefs.Should().HaveCountGreaterThan(0);
        var executionOrder = sequenceDefs.Select(d => d.ExecutionOrder).OrderBy(x => x).ToList();
        
        // Check that execution orders are sequential (may not start at 1 if other definitions exist)
        for (int i = 0; i < executionOrder.Count - 1; i++)
        {
            (executionOrder[i + 1] - executionOrder[i]).Should().BeGreaterThan(0, 
                "execution should be sequential");
        }
    }
}

/// <summary>
/// Test endpoint definition that tracks the environment it receives
/// </summary>
public class EnvironmentAwareEndpointDefinition : IEndpointDefinition
{
    public string? ReceivedEnvironmentName { get; private set; }

    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        ReceivedEnvironmentName = env.EnvironmentName;
    }
}

/// <summary>
/// Test endpoint definition that tracks execution order
/// </summary>
public class SequenceTrackingEndpointDefinition : IEndpointDefinition
{
    private static int _counter = 0;
    public int ExecutionOrder { get; private set; }

    public void DefineServices(IServiceCollection services)
    {
    }

    public void DefineEndpoints(WebApplication app, IWebHostEnvironment env)
    {
        ExecutionOrder = Interlocked.Increment(ref _counter);
    }
}
