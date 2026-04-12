using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class PecasInsumosControllerTests
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
            Nome = "Filtro de oleo",
            Codigo = "flt-001",
            Descricao = "Filtro para troca preventiva",
            PrecoUnitario = 45.90m,
            QuantidadeEstoque = 20
        };

        var response = await _client.PostAsJsonAsync("/api/pecasinsumos", request);
        var created = await response.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.Nome.Should().Be("Filtro de oleo");
            _ = created.Codigo.Should().Be("FLT-001");
            _ = created.Descricao.Should().Be("Filtro para troca preventiva");
            _ = created.PrecoUnitario.Should().Be(45.90m);
            _ = created.QuantidadeEstoque.Should().Be(20);
            _ = created.Ativo.Should().BeTrue();
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenCodigoAlreadyExists()
    {
        var firstRequest = new
        {
            Nome = "Oleo 5W30",
            Codigo = "OL-530",
            Descricao = "Lubrificante sintetico",
            PrecoUnitario = 59.90m,
            QuantidadeEstoque = 10
        };

        var secondRequest = new
        {
            Nome = "Oleo 5W30 Premium",
            Codigo = "ol-530",
            Descricao = "Mesmo codigo do item anterior",
            PrecoUnitario = 64.90m,
            QuantidadeEstoque = 6
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", firstRequest);
        var secondResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", secondRequest);
        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("codigo");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        var request = new
        {
            Nome = "  ",
            Codigo = "PI-100",
            Descricao = "Teste invalido",
            PrecoUnitario = 25m,
            QuantidadeEstoque = 2
        };

        var response = await _client.PostAsJsonAsync("/api/pecasinsumos", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("obrigatório");
        });
    }

    private sealed class PecaInsumoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int QuantidadeEstoque { get; set; }
        public bool Ativo { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
