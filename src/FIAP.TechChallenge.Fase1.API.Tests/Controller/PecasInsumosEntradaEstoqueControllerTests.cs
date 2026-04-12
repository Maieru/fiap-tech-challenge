using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class PecasInsumosEntradaEstoqueControllerTests
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
    public async Task PutEntradaEstoque_ShouldSucceed_WhenRequestIsValid()
    {
        var createRequest = new
        {
            Nome = "Filtro de oleo",
            Codigo = "FLT-002",
            Descricao = "Filtro para manutencao",
            PrecoUnitario = 49.90m,
            QuantidadeEstoque = 20
        };

        var createResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var entradaRequest = new
        {
            Quantidade = 5
        };

        var response = await _client.PutAsJsonAsync($"/api/pecasinsumos/{created!.Id}/entrada-estoque", entradaRequest);
        var updated = await response.Content.ReadFromJsonAsync<EntradaEstoqueResponse>();

        Assert.Multiple(() =>
        {
            _ = createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.Nome.Should().Be("Filtro de oleo");
            _ = updated.Codigo.Should().Be("FLT-002");
            _ = updated.QuantidadeEntrada.Should().Be(5);
            _ = updated.QuantidadeEstoque.Should().Be(25);
        });
    }

    [Test]
    public async Task PutEntradaEstoque_ShouldReturnNotFound_WhenPecaInsumoDoesNotExist()
    {
        var request = new
        {
            Quantidade = 4
        };

        var response = await _client.PutAsJsonAsync($"/api/pecasinsumos/{Guid.NewGuid()}/entrada-estoque", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("nao encontrado");
        });
    }

    [Test]
    public async Task PutEntradaEstoque_ShouldReturnBadRequest_WhenQuantidadeIsInvalid()
    {
        var createRequest = new
        {
            Nome = "Pasta de limpeza",
            Codigo = "PL-001",
            Descricao = "Pasta para limpeza de terminais",
            PrecoUnitario = 19.90m,
            QuantidadeEstoque = 8
        };

        var createResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var entradaRequest = new
        {
            Quantidade = 0
        };

        var response = await _client.PutAsJsonAsync($"/api/pecasinsumos/{created!.Id}/entrada-estoque", entradaRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("entrada em estoque");
        });
    }

    private sealed class PecaInsumoResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class EntradaEstoqueResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int QuantidadeEntrada { get; set; }
        public int QuantidadeEstoque { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
