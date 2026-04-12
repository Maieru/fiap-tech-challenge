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

    [Test]
    public async Task Update_ShouldSucceed_AndShouldNotChangeStock_WhenRequestContainsExtraStockField()
    {
        var createRequest = new
        {
            Nome = "Filtro de oleo",
            Codigo = "flt-200",
            Descricao = "Item para atualizacao",
            PrecoUnitario = 39.90m,
            QuantidadeEstoque = 20
        };

        var createResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var updateRequest = new
        {
            Nome = "Filtro de oleo premium",
            Codigo = "flt-201",
            Descricao = "Descricao atualizada",
            PrecoUnitario = 49.90m,
            Ativo = false,
            QuantidadeEstoque = 999
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/pecasinsumos/{created!.Id}", updateRequest);
        var updated = await updateResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        Assert.Multiple(() =>
        {
            _ = createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.Nome.Should().Be("Filtro de oleo premium");
            _ = updated.Codigo.Should().Be("FLT-201");
            _ = updated.Descricao.Should().Be("Descricao atualizada");
            _ = updated.PrecoUnitario.Should().Be(49.90m);
            _ = updated.QuantidadeEstoque.Should().Be(20);
            _ = updated.Ativo.Should().BeFalse();
        });
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenPecaInsumoDoesNotExist()
    {
        var request = new
        {
            Nome = "Filtro inexistente",
            Codigo = "FLT-909",
            Descricao = "Item inexistente",
            PrecoUnitario = 29.90m,
            Ativo = true
        };

        var response = await _client.PutAsJsonAsync($"/api/pecasinsumos/{Guid.NewGuid()}", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("encontrado");
        });
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenCodigoAlreadyExists()
    {
        var firstRequest = new
        {
            Nome = "Pastilha de freio",
            Codigo = "PST-100",
            Descricao = "Primeiro item",
            PrecoUnitario = 120m,
            QuantidadeEstoque = 7
        };

        var secondRequest = new
        {
            Nome = "Disco de freio",
            Codigo = "DSC-200",
            Descricao = "Segundo item",
            PrecoUnitario = 300m,
            QuantidadeEstoque = 4
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", firstRequest);
        var secondResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", secondRequest);
        var secondCreated = await secondResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var updateRequest = new
        {
            Nome = "Disco de freio",
            Codigo = "pst-100",
            Descricao = "Tentativa com codigo duplicado",
            PrecoUnitario = 300m,
            Ativo = true
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/pecasinsumos/{secondCreated!.Id}", updateRequest);
        var error = await updateResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = secondCreated.Should().NotBeNull();
            _ = updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("codigo");
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
