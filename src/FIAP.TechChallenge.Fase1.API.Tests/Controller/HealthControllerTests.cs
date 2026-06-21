using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class HealthControllerTests
{
    private string? _jwtSigningKey;
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _jwtSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-signing-key-with-at-least-32-bytes");

        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        Environment.SetEnvironmentVariable("Jwt__SigningKey", _jwtSigningKey);

        _client?.Dispose();

        if (_factory is not null)
            await _factory.DisposeAsync();
    }

    [Test]
    public async Task Get_ShouldReturnInstanceDependenciesAndMissingEnvironmentVariables()
    {
        var response = await _client.GetAsync("/api/health");
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = health.Should().NotBeNull();
            _ = health!.InstanceId.Should().NotBeEmpty();
            _ = health.Dependencies.Should().ContainSingle(dependency =>
                dependency.Name == "database" && dependency.Status == "Healthy");
            _ = health.EnvironmentVariables.Should().NotContain(variable =>
                variable.Name == "Jwt__SigningKey");
            _ = health.EnvironmentVariables.Should().OnlyContain(variable => !variable.Exists);
        });
    }

    private sealed class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public Guid InstanceId { get; set; }
        public List<DependencyStatus> Dependencies { get; set; } = [];
        public List<EnvironmentVariableStatus> EnvironmentVariables { get; set; } = [];
    }

    private sealed class DependencyStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    private sealed class EnvironmentVariableStatus
    {
        public string Name { get; set; } = string.Empty;
        public bool Exists { get; set; }
    }
}

