using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class ServicosControllerTests
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
    public async Task Create_ShouldSucceed_WhenRequestIsValid()
    {
        var request = new
        {
            Descricao = "  Alinhamento e balanceamento  ",
            ValorUnitario = 199.90m
        };

        var response = await _client.PostAsJsonAsync("/api/servicos", request);
        var created = await response.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.Descricao.Should().Be("Alinhamento e balanceamento");
            _ = created.ValorUnitario.Should().Be(199.90m);
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDescricaoIsInvalid()
    {
        var request = new
        {
            Descricao = "   ",
            ValorUnitario = 50m
        };

        var response = await _client.PostAsJsonAsync("/api/servicos", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("obrigatória");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenValorUnitarioIsNegative()
    {
        var request = new
        {
            Descricao = "Troca de óleo",
            ValorUnitario = -1m
        };

        var response = await _client.PostAsJsonAsync("/api/servicos", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("não pode ser negativo");
        });
    }

    private sealed class ServicoResponse
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
