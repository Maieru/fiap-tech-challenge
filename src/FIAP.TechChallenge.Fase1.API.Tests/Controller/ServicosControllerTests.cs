using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class ServicosControllerTests
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
    public async Task CreateUpdate_ShouldSucceed_WhenRequestIsValid()
    {
        var createRequest = new
        {
            Descricao = "  Alinhamento e balanceamento  ",
            ValorUnitario = 199.90m
        };

        var postResponse = await _client.PostAsJsonAsync("/api/servicos", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.Descricao.Should().Be("Alinhamento e balanceamento");
            _ = created.ValorUnitario.Should().Be(199.90m);
        });

        var updateRequest = new
        {
            Descricao = "Troca de velas",
            ValorUnitario = 249.50m
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/servicos/{created!.Id}", updateRequest);
        var updated = await putResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.Descricao.Should().Be("Troca de velas");
            _ = updated.ValorUnitario.Should().Be(249.50m);
        });
    }

    [Test]
    public async Task Get_ShouldListAllAndGetById_WhenRequestIsValid()
    {
        var firstCreateRequest = new
        {
            Descricao = "Alinhamento",
            ValorUnitario = 99.90m
        };

        var secondCreateRequest = new
        {
            Descricao = "Balanceamento",
            ValorUnitario = 79.90m
        };

        var firstCreateResponse = await _client.PostAsJsonAsync("/api/servicos", firstCreateRequest);
        var secondCreateResponse = await _client.PostAsJsonAsync("/api/servicos", secondCreateRequest);
        var firstCreated = await firstCreateResponse.Content.ReadFromJsonAsync<ServicoResponse>();
        var secondCreated = await secondCreateResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        var getAllResponse = await _client.GetAsync("/api/servicos?pageNumber=1&pageSize=10");
        var allResult = await getAllResponse.Content.ReadFromJsonAsync<ListarServicosResponse>();

        var getByIdResponse = await _client.GetAsync($"/api/servicos/{firstCreated!.Id}");
        var byId = await getByIdResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = firstCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = secondCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            _ = getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = allResult.Should().NotBeNull();
            _ = allResult!.Servicos.Count.Should().BeGreaterThanOrEqualTo(2);
            _ = allResult.Servicos.Any(x => x.Id == firstCreated.Id).Should().BeTrue();
            _ = allResult.Servicos.Any(x => x.Id == secondCreated!.Id).Should().BeTrue();

            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byId.Should().NotBeNull();
            _ = byId!.Id.Should().Be(firstCreated.Id);
            _ = byId.Descricao.Should().Be("Alinhamento");
            _ = byId.ValorUnitario.Should().Be(99.90m);
        });
    }

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenServicoDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/servicos/{Guid.NewGuid()}");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Servico nao encontrado.");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Get_ShouldReturnBadRequest_WhenPageNumberIsZero()
    {
        var response = await _client.GetAsync("/api/servicos?pageNumber=0&pageSize=10");
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
            _ = error!.Error.Should().Contain("obrigat");
            _ = error.ErrorCode.Should().Be("BadRequest");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenValorUnitarioIsNegative()
    {
        var request = new
        {
            Descricao = "Troca de oleo",
            ValorUnitario = -1m
        };

        var response = await _client.PostAsJsonAsync("/api/servicos", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("negativo");
        });
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenServicoDoesNotExist()
    {
        var request = new
        {
            Descricao = "Troca de velas",
            ValorUnitario = 200m
        };

        var response = await _client.PutAsJsonAsync($"/api/servicos/{Guid.NewGuid()}", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Servico nao encontrado.");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenDescricaoIsInvalid()
    {
        var createRequest = new
        {
            Descricao = "Troca de filtro",
            ValorUnitario = 120m
        };

        var postResponse = await _client.PostAsJsonAsync("/api/servicos", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        _ = created.Should().NotBeNull();

        var updateRequest = new
        {
            Descricao = " ",
            ValorUnitario = 130m
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/servicos/{created!.Id}", updateRequest);
        var error = await putResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = putResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("obrigat");
            _ = error.ErrorCode.Should().Be("BadRequest");
        });
    }

    [Test]
    public async Task AllActions_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var getResponse = await SendUnauthorizedAsync(HttpMethod.Get, "/api/servicos");
        var getByIdResponse = await SendUnauthorizedAsync(HttpMethod.Get, $"/api/servicos/{Guid.NewGuid()}");
        var postResponse = await SendUnauthorizedAsync(HttpMethod.Post, "/api/servicos", new
        {
            Descricao = "Alinhamento",
            ValorUnitario = 99.90m
        });
        var putResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/servicos/{Guid.NewGuid()}", new
        {
            Descricao = "Balanceamento",
            ValorUnitario = 79.90m
        });

        Assert.Multiple(() =>
        {
            _ = getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

    private sealed class ServicoResponse
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }

    private sealed class ListarServicosResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public IReadOnlyCollection<ServicoResponse> Servicos { get; set; } = [];
    }
}

