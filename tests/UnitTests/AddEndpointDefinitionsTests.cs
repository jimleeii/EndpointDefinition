using EndpointDefinition.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointDefinition.Tests;

public class AddEndpointDefinitionsTests
{
    [Fact]
    public void AddEndpointDefinitions_ShouldThrowArgumentNullException_WhenScanMarkersIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddEndpointDefinitions(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("scanMarkers");
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldThrowArgumentNullException_WhenScanMarkersIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddEndpointDefinitions(Array.Empty<Type>());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("scanMarkers");
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldDiscoverEndpointDefinitionsFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpointDefinitions(typeof(TestEndpointDefinition));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var definitions = serviceProvider.GetService<IReadOnlyCollection<IEndpointDefinition>>();
        
        definitions.Should().NotBeNull();
        definitions.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldRegisterServicesDefinedByEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpointDefinitions(typeof(TestEndpointDefinition));

        // Assert - Verify that services registered by endpoint definitions are present
        services.Should().Contain(sd => sd.ServiceType == typeof(TestService));
        services.Should().Contain(sd => sd.ServiceType == typeof(AnotherTestService));
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldCallDefineServicesOnAllDefinitions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpointDefinitions(typeof(TestEndpointDefinition));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Verify that TestService was registered by TestEndpointDefinition
        var testService = serviceProvider.GetService<TestService>();
        testService.Should().NotBeNull();

        // Verify that AnotherTestService was registered by AnotherTestEndpointDefinition
        var anotherTestService = serviceProvider.GetService<AnotherTestService>();
        anotherTestService.Should().NotBeNull();
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldRegisterDefinitionsAsReadOnlyCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpointDefinitions(typeof(TestEndpointDefinition));

        // Assert
        services.Should().Contain(sd => 
            sd.ServiceType == typeof(IReadOnlyCollection<IEndpointDefinition>) && 
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldHandleMultipleScanMarkers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpointDefinitions(
            typeof(TestEndpointDefinition), 
            typeof(AnotherTestEndpointDefinition));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var definitions = serviceProvider.GetService<IReadOnlyCollection<IEndpointDefinition>>();
        
        definitions.Should().NotBeNull();
        definitions.Should().Contain(d => d.GetType() == typeof(TestEndpointDefinition));
        definitions.Should().Contain(d => d.GetType() == typeof(AnotherTestEndpointDefinition));
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldNotRegisterDuplicateDefinitions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Register the same assembly twice
        services.AddEndpointDefinitions(
            typeof(TestEndpointDefinition), 
            typeof(TestEndpointDefinition));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var definitions = serviceProvider.GetService<IReadOnlyCollection<IEndpointDefinition>>();
        
        definitions.Should().NotBeNull();
        var testDefinitions = definitions!.Where(d => d.GetType() == typeof(TestEndpointDefinition)).ToList();
        testDefinitions.Should().HaveCount(1);
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldNotRegisterInterfacesOrAbstractClasses()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpointDefinitions(typeof(IEndpointDefinition));

        // Assert
        services.Should().NotContain(sd => 
            sd.ServiceType == typeof(IEndpointDefinition) && 
            sd.ImplementationType == typeof(IEndpointDefinition));
    }

    [Fact]
    public void AddEndpointDefinitions_ShouldHandleNoEndpointDefinitionsFound()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Use a type from an assembly with no IEndpointDefinition implementations
        services.AddEndpointDefinitions(typeof(string));

        // Assert - Should not throw and should not register anything
        var serviceProvider = services.BuildServiceProvider();
        var definitions = serviceProvider.GetService<IReadOnlyCollection<IEndpointDefinition>>();
        
        definitions.Should().BeNull();
    }
}
