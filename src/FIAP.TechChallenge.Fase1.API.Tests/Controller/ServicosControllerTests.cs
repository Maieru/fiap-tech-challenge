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
