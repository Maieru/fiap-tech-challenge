using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests;

internal static class TestAuthenticationHelper
{
    private const string DefaultPassword = "SenhaForte@123";

    public static async Task ConfigureAuthenticatedClientAsync(HttpClient client)
    {
        var userName = $"test-user-{Guid.NewGuid():N}";
        var createUserRequest = new
        {
            Usuario = userName,
            Senha = DefaultPassword
        };

        var createUserResponse = await client.PostAsJsonAsync("/api/usuarios", createUserRequest);

        if (createUserResponse.StatusCode != HttpStatusCode.Created)
            throw new InvalidOperationException($"Nao foi possivel criar usuario de teste para autenticacao. Status: {createUserResponse.StatusCode}");

        var loginRequest = new
        {
            Usuario = userName,
            Senha = DefaultPassword
        };

        var loginResponse = await client.PostAsJsonAsync("/api/usuarios/login", loginRequest);

        if (loginResponse.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"Nao foi possivel autenticar usuario de teste. Status: {loginResponse.StatusCode}");

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        if (string.IsNullOrWhiteSpace(loginResult?.Token))
            throw new InvalidOperationException("O endpoint de login nao retornou token valido para os testes.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(loginResult.TipoToken, loginResult.Token);
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TipoToken { get; set; } = string.Empty;
    }
}
