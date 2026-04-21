using FluentAssertions;
using FIAP.TechChallenge.Fase1.API.Tests;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class PecasInsumosControllerTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _unauthorizedClient = null!;

    [SetUp]
    public async Task SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _unauthorizedClient = _factory.CreateClient();
        await TestAuthenticationHelper.ConfigureAuthenticatedClientAsync(_client);
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();
        _unauthorizedClient.Dispose();
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
    public async Task Get_ShouldListAllGetByIdAndGetByCodigo_WhenFiltersAreValid()
    {
        var firstCreateRequest = new
        {
            Nome = "Filtro de oleo",
            Codigo = "flt-500",
            Descricao = "Filtro para primeira listagem",
            PrecoUnitario = 45.90m,
            QuantidadeEstoque = 20
        };

        var secondCreateRequest = new
        {
            Nome = "Pastilha de freio",
            Codigo = "pst-501",
            Descricao = "Pastilha dianteira",
            PrecoUnitario = 120m,
            QuantidadeEstoque = 8
        };

        var firstCreateResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", firstCreateRequest);
        var secondCreateResponse = await _client.PostAsJsonAsync("/api/pecasinsumos", secondCreateRequest);
        var firstCreated = await firstCreateResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();
        var secondCreated = await secondCreateResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var getAllResponse = await _client.GetAsync("/api/pecasinsumos?pageNumber=1&pageSize=10");
        var allResult = await getAllResponse.Content.ReadFromJsonAsync<ListarPecasInsumosResponse>();

        var getByIdResponse = await _client.GetAsync($"/api/pecasinsumos/{firstCreated!.Id}");
        var byId = await getByIdResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var getByCodigoResponse = await _client.GetAsync("/api/pecasinsumos?codigo=pst-501");
        var byCodigo = await getByCodigoResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        Assert.Multiple(() =>
        {
            _ = firstCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = secondCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            _ = getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = allResult.Should().NotBeNull();
            _ = allResult!.PecasInsumos.Count.Should().BeGreaterThanOrEqualTo(2);
            _ = allResult.PecasInsumos.Any(x => x.Id == firstCreated.Id).Should().BeTrue();
            _ = allResult.PecasInsumos.Any(x => x.Id == secondCreated!.Id).Should().BeTrue();

            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byId.Should().NotBeNull();
            _ = byId!.Id.Should().Be(firstCreated.Id);
            _ = byId.Codigo.Should().Be("FLT-500");

            _ = getByCodigoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byCodigo.Should().NotBeNull();
            _ = byCodigo!.Id.Should().Be(secondCreated!.Id);
            _ = byCodigo.Codigo.Should().Be("PST-501");
        });
    }

    [Test]
    public async Task GetByCodigo_ShouldReturnNotFound_WhenPecaInsumoDoesNotExist()
    {
        var response = await _client.GetAsync("/api/pecasinsumos?codigo=nao-existe");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("encontrado");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Get_ShouldReturnBadRequest_WhenPageNumberIsZero()
    {
        var response = await _client.GetAsync("/api/pecasinsumos?pageNumber=0&pageSize=10");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("O numero da pagina deve ser maior que zero.");
            _ = error.ErrorCode.Should().Be("BadRequest");
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
            _ = error.ErrorCode.Should().Be("NotFound");
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
            _ = error.ErrorCode.Should().Be("NotFound");
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

    [Test]
    public async Task AllActions_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var getResponse = await SendUnauthorizedAsync(HttpMethod.Get, "/api/pecasinsumos");
        var getByIdResponse = await SendUnauthorizedAsync(HttpMethod.Get, $"/api/pecasinsumos/{Guid.NewGuid()}");
        var postResponse = await SendUnauthorizedAsync(HttpMethod.Post, "/api/pecasinsumos", new
        {
            Nome = "Filtro de oleo",
            Codigo = "flt-unauth",
            Descricao = "Teste sem token",
            PrecoUnitario = 10m,
            QuantidadeEstoque = 1
        });
        var putEntradaEstoqueResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/pecasinsumos/{Guid.NewGuid()}/entrada-estoque", new
        {
            Quantidade = 2
        });
        var putResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/pecasinsumos/{Guid.NewGuid()}", new
        {
            Nome = "Filtro atualizado",
            Codigo = "flt-unauth-upd",
            Descricao = "Atualizacao sem token",
            PrecoUnitario = 12m,
            Ativo = true
        });

        Assert.Multiple(() =>
        {
            _ = getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = putEntradaEstoqueResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        });
    }

    private async Task<HttpResponseMessage> SendUnauthorizedAsync(HttpMethod method, string uri, object? payload = null)
    {
        using var request = new HttpRequestMessage(method, uri);

        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        return await _unauthorizedClient.SendAsync(request);
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
        public string ErrorCode { get; set; } = string.Empty;
    }

    private sealed class ListarPecasInsumosResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public IReadOnlyCollection<PecaInsumoResponse> PecasInsumos { get; set; } = [];
    }

    private sealed class EntradaEstoqueResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int QuantidadeEntrada { get; set; }
        public int QuantidadeEstoque { get; set; }
    }
}


