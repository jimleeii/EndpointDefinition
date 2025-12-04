using EndpointDefinition;

var builder = WebApplication.CreateBuilder(args);

// Add endpoint definitions
builder.Services.AddEndpointDefinitions(typeof(IntegrationTests.TestProgram));

var app = builder.Build();

// Use endpoint definitions
app.UseEndpointDefinitions(app.Environment);

app.Run();

// Make the implicit Program class public and partial for WebApplicationFactory
public partial class Program { }
