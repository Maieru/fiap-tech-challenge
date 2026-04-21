using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class UsuariosControllerTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task Post_ShouldCreateUser_WhenRequestIsValid()
    {
        var request = new
        {
            Usuario = "Admin",
            Senha = "SenhaForte@123"
        };

        var response = await _client.PostAsJsonAsync("/api/usuarios", request);
        var created = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.Usuario.Should().Be("admin");
            _ = created.SenhaCriptografada.Should().StartWith("$2");
        });
    }

    [Test]
    public async Task Post_ShouldReturnConflict_WhenUsuarioAlreadyExists()
    {
        var request = new
        {
            Usuario = "Admin",
            Senha = "SenhaForte@123"
        };

        _ = await _client.PostAsJsonAsync("/api/usuarios", request);

        var secondResponse = await _client.PostAsJsonAsync("/api/usuarios", request);
        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Já existe um usuário cadastrado com este nome de usuário.");
            _ = error.ErrorCode.Should().Be("Conflict");
        });
    }

    [Test]
    public async Task Post_ShouldReturnBadRequest_WhenSenhaIsInvalid()
    {
        var request = new
        {
            Usuario = "Admin",
            Senha = "123"
        };

        var response = await _client.PostAsJsonAsync("/api/usuarios", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("A senha deve ter no mínimo 8 caracteres.");
            _ = error.ErrorCode.Should().Be("BadRequest");
        });
    }

    private sealed class UsuarioResponse
    {
        public Guid Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string SenhaCriptografada { get; set; } = string.Empty;
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }
}
