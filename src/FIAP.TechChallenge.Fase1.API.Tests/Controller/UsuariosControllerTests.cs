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
        });
    }

    [Test]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var createRequest = new
        {
            Usuario = "Admin",
            Senha = "SenhaForte@123"
        };

        _ = await _client.PostAsJsonAsync("/api/usuarios", createRequest);

        var loginRequest = new
        {
            Usuario = "Admin",
            Senha = "SenhaForte@123"
        };

        var response = await _client.PostAsJsonAsync("/api/usuarios/login", loginRequest);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = auth.Should().NotBeNull();
            _ = auth!.Token.Should().NotBeNullOrWhiteSpace();
            _ = auth.TipoToken.Should().Be("Bearer");
            _ = auth.ExpiresInSeconds.Should().BeGreaterThan(0);
        });
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var request = new
        {
            Usuario = "Admin",
            Senha = "SenhaErrada@123"
        };

        var response = await _client.PostAsJsonAsync("/api/usuarios/login", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Usuario ou senha invalidos.");
            _ = error.ErrorCode.Should().Be("Unauthorized");
        });
    }

    [Test]
    public async Task Delete_ShouldSoftDeleteUser_WhenUserExists()
    {
        var usuario = $"delete-user-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/usuarios", new
        {
            Usuario = usuario,
            Senha = "SenhaForte@123"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        await TestAuthenticationHelper.ConfigureAuthenticatedClientAsync(_client);

        var deleteResponse = await _client.DeleteAsync($"/api/usuarios/{created!.Id}");
        var loginResponse = await _client.PostAsJsonAsync("/api/usuarios/login", new
        {
            Usuario = usuario,
            Senha = "SenhaForte@123"
        });

        Assert.Multiple(() =>
        {
            _ = createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _ = loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        });
    }

    [Test]
    public async Task Delete_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var response = await _client.DeleteAsync($"/api/usuarios/{Guid.NewGuid()}");

        _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

    [Test]
    public async Task Post_ShouldReturnBadRequest_WhenUsuarioHasUnsafeCharacters()
    {
        var request = new
        {
            Usuario = "John Doe AND 1=1 --",
            Senha = "SenhaForte@123"
        };

        var response = await _client.PostAsJsonAsync("/api/usuarios", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("O usuario deve conter apenas letras, numeros, ponto, hifen ou underscore.");
            _ = error.ErrorCode.Should().Be("BadRequest");
        });
    }

    private sealed class UsuarioResponse
    {
        public Guid Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    private sealed class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TipoToken { get; set; } = string.Empty;
        public int ExpiresInSeconds { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }
}

