using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class EndpointDefinitionIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndpointDefinitionIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task WeatherEndpoint_ShouldReturnWeather()
    {
        // Act
        var response = await _client.GetAsync("/weather");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Sunny");
    }

    [Fact]
    public async Task WeatherEndpointWithCity_ShouldReturnCityWeather()
    {
        // Act
        var response = await _client.GetAsync("/weather/Seattle");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Seattle");
        content.Should().Contain("Cloudy");
    }

    [Fact]
    public async Task UsersEndpoint_ShouldReturnUserList()
    {
        // Act
        var response = await _client.GetAsync("/users");
        var users = await response.Content.ReadFromJsonAsync<List<User>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        users.Should().NotBeNull();
        users.Should().HaveCount(2);
        users.Should().Contain(u => u.Name == "John Doe");
        users.Should().Contain(u => u.Name == "Jane Smith");
    }

    [Fact]
    public async Task UsersEndpointById_ShouldReturnSpecificUser()
    {
        // Act
        var response = await _client.GetAsync("/users/1");
        var user = await response.Content.ReadFromJsonAsync<User>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        user.Should().NotBeNull();
        user!.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task UsersEndpointById_ShouldReturnNotFoundForInvalidId()
    {
        // Act
        var response = await _client.GetAsync("/users/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostUserEndpoint_ShouldCreateNewUser()
    {
        // Arrange
        var newUser = new User { Id = 3, Name = "Bob Johnson" };

        // Act
        var response = await _client.PostAsJsonAsync("/users", newUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be("/users/3");
    }

    [Fact]
    public async Task DebugEndpoint_ShouldBeAvailableInDevelopment()
    {
        // Act
        var response = await _client.GetAsync("/debug/info");
        var info = await response.Content.ReadFromJsonAsync<DebugInfo>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        info.Should().NotBeNull();
        info!.Environment.Should().Be("Development");
    }

    [Fact]
    public async Task MultipleEndpointDefinitions_ShouldAllBeRegistered()
    {
        // Arrange - Test that all endpoint definitions were discovered and registered

        // Act & Assert - Weather endpoints
        var weatherResponse = await _client.GetAsync("/weather");
        weatherResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - User endpoints
        var usersResponse = await _client.GetAsync("/users");
        usersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Debug endpoints
        var debugResponse = await _client.GetAsync("/debug/info");
        debugResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ServicesRegisteredByEndpointDefinitions_ShouldBeAvailable()
    {
        // This test verifies that services registered in DefineServices are properly injected
        // into the endpoint handlers

        // Act - Call endpoint that requires IWeatherService
        var weatherResponse = await _client.GetAsync("/weather");
        
        // Assert - If service wasn't registered, this would fail
        weatherResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Call endpoint that requires IUserService
        var usersResponse = await _client.GetAsync("/users");
        
        // Assert - If service wasn't registered, this would fail
        usersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// Helper class for debug endpoint response
public class DebugInfo
{
    public string Environment { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
}
