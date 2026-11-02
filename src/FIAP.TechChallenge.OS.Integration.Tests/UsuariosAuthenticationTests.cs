using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[NonParallelizable]
public sealed class UsuariosAuthenticationTests
{
    private OsTestHost host = null!;
    private const string Users = OsTestHost.Root + "/usuarios";
    [OneTimeSetUp] public async Task Start() { host = new(); await host.StartAsync(); }
    [SetUp] public async Task Reset() { await host.ResetAsync(); host.Client.DefaultRequestHeaders.Authorization = null; }
    [OneTimeTearDown] public async Task Stop() => await host.DisposeAsync();

    [TestCase("SenhaForte@123")]
    [TestCase(" SenhaForte@123 ")]
    public async Task RegisterLoginAndSoftDeleteWithRealPasswordHash(string password)
    {
        var credentials = new { usuario = " Admin ", senha = password };
        using var created = await host.Client.PostAsJsonAsync(Users, credentials);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Multiple(() => { Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created)); Assert.That(body.GetProperty("usuario").GetString(), Is.EqualTo("admin")); Assert.That(body.TryGetProperty("senha", out _), Is.False); });
        var id = body.GetProperty("id").GetGuid();
        await using (var db = host.CreateDbContext())
        {
            var stored = await db.Usuarios.SingleAsync(x => x.Id == id);
            Assert.Multiple(() => { Assert.That(stored.Senha, Is.Not.EqualTo(password)); Assert.That(BCrypt.Net.BCrypt.Verify(password, stored.Senha), Is.True); });
        }
        using var login = await host.Client.PostAsJsonAsync(Users + "/login", credentials);
        var response = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = response.GetProperty("token").GetString()!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Multiple(() => { Assert.That(login.StatusCode, Is.EqualTo(HttpStatusCode.OK)); Assert.That(response.GetProperty("expiresInSeconds").GetInt32(), Is.EqualTo(3600)); Assert.That(response.GetProperty("tipoToken").GetString(), Is.EqualTo("Bearer")); Assert.That(jwt.Subject, Is.EqualTo(id.ToString())); Assert.That(jwt.Header.Alg, Is.EqualTo("HS256")); });
        using var anonymousDelete = await host.Client.DeleteAsync(Users + "/" + id);
        Assert.That(anonymousDelete.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.That((await host.Client.GetAsync(OsTestHost.Root + "/ordens")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await host.Client.DeleteAsync(Users + "/" + id)).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await host.Client.PostAsJsonAsync(Users + "/login", credentials)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        // Existing stateless JWTs remain valid until expiration; deletion prevents new logins.
        Assert.That((await host.Client.GetAsync(OsTestHost.Root + "/ordens")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test] public async Task ConcurrentNormalizedRegistrationHasOneWinner()
    {
        var replies = await Task.WhenAll(host.Client.PostAsJsonAsync(Users, new { usuario = "ADMIN", senha = "SenhaForte@123" }), host.Client.PostAsJsonAsync(Users, new { usuario = " admin ", senha = "SenhaForte@123" }));
        try { Assert.Multiple(() => { Assert.That(replies.Count(x => x.StatusCode == HttpStatusCode.Created), Is.EqualTo(1)); Assert.That(replies.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1)); }); }
        finally { foreach (var reply in replies) reply.Dispose(); }
    }

    [Test] public async Task WrongAndUnknownCredentialsHaveSameResponse()
    {
        await host.Client.PostAsJsonAsync(Users, new { usuario = "admin", senha = "SenhaForte@123" });
        using var wrong = await host.Client.PostAsJsonAsync(Users + "/login", new { usuario = "admin", senha = "wrong-password" });
        using var missing = await host.Client.PostAsJsonAsync(Users + "/login", new { usuario = "unknown", senha = "wrong-password" });
        Assert.Multiple(() => { Assert.That(wrong.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized)); Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized)); Assert.That(wrong.Content.ReadAsStringAsync().Result, Is.EqualTo(missing.Content.ReadAsStringAsync().Result)); });
    }

    [TestCase("bad login", "SenhaForte@123")]
    [TestCase("admin", "short")]
    public async Task RejectInvalidRegistration(string user, string password) => Assert.That((await host.Client.PostAsJsonAsync(Users, new { usuario = user, senha = password })).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    [Test] public async Task RejectPasswordBeyondBcryptLimit() => Assert.That((await host.Client.PostAsJsonAsync(Users, new { usuario = "admin", senha = new string('é', 37) })).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
}
