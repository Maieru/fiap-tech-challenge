using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class ServicosControllerTests
{
    private static int _seedCounter = 9500;
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
    public async Task Delete_ShouldSoftDeleteServico_WhenServicoExists()
    {
        var servico = await CreateServicoAsync("Servico para exclusao", 120m);

        var deleteResponse = await _client.DeleteAsync($"/api/servicos/{servico.Id}");
        var getResponse = await _client.GetAsync($"/api/servicos/{servico.Id}");

        Assert.Multiple(() =>
        {
            _ = deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _ = getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
    public async Task GetTempoMedio_ShouldReturnExecutionSummary_WhenServicoHasNoCompletedExecution()
    {
        var createRequest = new
        {
            Descricao = "Troca de pastilhas",
            ValorUnitario = 180m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/servicos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServicoResponse>();

        _ = created.Should().NotBeNull();

        var response = await _client.GetAsync($"/api/servicos/{created!.Id}/tempo-medio");
        var body = await response.Content.ReadFromJsonAsync<TempoMedioServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = body.Should().NotBeNull();
            _ = body!.ServicoId.Should().Be(created.Id);
            _ = body.QuantidadeExecucoes.Should().Be(0);
            _ = body.TempoMedioMinutos.Should().Be(0m);
        });
    }

    [Test]
    public async Task GetTempoMedio_ShouldReturnExecutionSummary_WhenServicoHasCompletedExecutions()
    {
        var servico = await CreateServicoAsync("Troca de bateria", 350m);
        var cliente = await CreateClientAsync();
        var veiculo = await CreateVehicleAsync(cliente.Id);

        var ordem1 = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha na ignicao");
        var ordem2 = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Queda de tensao na partida");

        await ConcluirServicoNaOrdemAsync(ordem1.Id, servico.Id, 30);
        await ConcluirServicoNaOrdemAsync(ordem2.Id, servico.Id, 50);

        var response = await _client.GetAsync($"/api/servicos/{servico.Id}/tempo-medio");
        var body = await response.Content.ReadFromJsonAsync<TempoMedioServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = body.Should().NotBeNull();
            _ = body!.ServicoId.Should().Be(servico.Id);
            _ = body.QuantidadeExecucoes.Should().Be(2);
            _ = body.TempoMedioMinutos.Should().Be(40m);
        });
    }

    [Test]
    public async Task GetTempoMedio_ShouldReturnNotFound_WhenServicoDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/servicos/{Guid.NewGuid()}/tempo-medio");
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
    public async Task AllActions_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var getResponse = await SendUnauthorizedAsync(HttpMethod.Get, "/api/servicos");
        var getByIdResponse = await SendUnauthorizedAsync(HttpMethod.Get, $"/api/servicos/{Guid.NewGuid()}");
        var getTempoMedioResponse = await SendUnauthorizedAsync(HttpMethod.Get, $"/api/servicos/{Guid.NewGuid()}/tempo-medio");
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
        var deleteResponse = await SendUnauthorizedAsync(HttpMethod.Delete, $"/api/servicos/{Guid.NewGuid()}");

        Assert.Multiple(() =>
        {
            _ = getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = getTempoMedioResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        });
    }

    private async Task<HttpResponseMessage> SendUnauthorizedAsync(HttpMethod method, string uri, object? payload = null)
    {
        using var request = new HttpRequestMessage(method, uri);

        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        return await _unauthorizedClient.SendAsync(request);
    }

    private async Task<ServicoResponse> CreateServicoAsync(string descricao, decimal valorUnitario)
    {
        var request = new
        {
            Descricao = descricao,
            ValorUnitario = valorUnitario
        };

        var response = await _client.PostAsJsonAsync("/api/servicos", request);
        var created = await response.Content.ReadFromJsonAsync<ServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
        });

        return created!;
    }

    private async Task<ClienteResponse> CreateClientAsync()
    {
        var seed = System.Threading.Interlocked.Increment(ref _seedCounter);

        var request = new
        {
            Nome = $"Cliente Servico {seed}",
            Cpf = GenerateValidCpf(seed),
            Telefone = $"1198{seed:D7}",
            Email = $"cliente.servico.{seed}@email.com"
        };

        var response = await _client.PostAsJsonAsync("/api/clientes", request);
        var created = await response.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
        });

        return created!;
    }

    private async Task<VeiculoResponse> CreateVehicleAsync(Guid clienteId)
    {
        var seed = System.Threading.Interlocked.Increment(ref _seedCounter);

        var request = new
        {
            ClienteId = clienteId,
            Placa = GenerateValidPlaca(seed),
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2025
        };

        var response = await _client.PostAsJsonAsync("/api/veiculos", request);
        var created = await response.Content.ReadFromJsonAsync<VeiculoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
        });

        return created!;
    }

    private async Task<OrdemServicoResponse> CreateOrdemServicoAsync(Guid clienteId, Guid veiculoId, string descricaoProblema)
    {
        var request = new
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            DescricaoProblema = descricaoProblema
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico", request);
        var created = await response.Content.ReadFromJsonAsync<OrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
        });

        return created!;
    }

    private async Task ConcluirServicoNaOrdemAsync(Guid ordemServicoId, Guid servicoId, int tempoGastoMinutos)
    {
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServicoId}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var addServicoResponse = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServicoId}/addservico", new
        {
            ServicoId = servicoId,
            Quantidade = 1
        });
        var servicoDaOrdem = await addServicoResponse.Content.ReadFromJsonAsync<ServicoDaOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = addServicoResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = servicoDaOrdem.Should().NotBeNull();
            _ = servicoDaOrdem!.Id.Should().NotBeEmpty();
        });

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServicoId}/solicitar-aprovacao", new { });
        _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var aprovarExecucaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServicoId}/aprovar-execucao", new { });
        _ = aprovarExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var concluirServicoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/servicos/{servicoDaOrdem!.Id}/concluir", new
        {
            TempoGastoMinutos = tempoGastoMinutos
        });

        _ = concluirServicoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string GenerateValidPlaca(int seed)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var first = alphabet[seed / 676 % 26];
        var second = alphabet[seed / 26 % 26];
        var third = alphabet[seed % 26];
        var digits = (seed % 10000).ToString("D4");

        return $"{first}{second}{third}{digits}";
    }

    private static string GenerateValidCpf(int seed)
    {
        var baseNumber = (seed % 999999999).ToString("D9");
        var digits = baseNumber.Select(c => c - '0').ToArray();

        var firstDigit = CalculateCpfDigit(digits, 10);
        var secondDigit = CalculateCpfDigit(digits.Append(firstDigit).ToArray(), 11);

        return $"{baseNumber}{firstDigit}{secondDigit}";
    }

    private static int CalculateCpfDigit(int[] digits, int weightStart)
    {
        var sum = 0;
        var weight = weightStart;

        foreach (var digit in digits)
        {
            sum += digit * weight;
            weight--;
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private sealed class ServicoResponse
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
    }

    private sealed class ClienteResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class VeiculoResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class OrdemServicoResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class ServicoDaOrdemServicoResponse
    {
        public Guid Id { get; set; }
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

    private sealed class TempoMedioServicoResponse
    {
        public Guid ServicoId { get; set; }
        public decimal TempoMedioMinutos { get; set; }
        public int QuantidadeExecucoes { get; set; }
    }
}

