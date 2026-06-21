using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class OrdensServicoControllerTests
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
        var cliente = await CreateClientAsync(9001);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(31), "Toyota", "Yaris", 2022);
        var request = new
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            DescricaoProblema = "Barulho na suspensao dianteira."
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico", request);
        var created = await response.Content.ReadFromJsonAsync<OrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.ClienteId.Should().Be(cliente.Id);
            _ = created.VeiculoId.Should().Be(veiculo.Id);
            _ = created.DescricaoProblema.Should().Be("Barulho na suspensao dianteira.");
            _ = created.Status.Should().Be(1);
            _ = created.DataCriacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });
    }

    [Test]
    public async Task CreateComClienteEVeiculo_ShouldSucceed_WhenRequestIsValid()
    {
        var request = new
        {
            Cliente = new
            {
                Nome = "Cliente OS Completa",
                Cpf = GenerateValidCpf(9101),
                Telefone = "11989101001",
                Email = "cliente.os.completa@email.com"
            },
            Veiculo = new
            {
                Placa = GenerateValidPlaca(71),
                Marca = "Honda",
                Modelo = "Fit",
                Ano = 2021
            },
            DescricaoProblema = "Cliente informou falha ao ligar pela manha."
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico/com-cliente-veiculo", request);
        var created = await response.Content.ReadFromJsonAsync<OrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.ClienteId.Should().NotBeEmpty();
            _ = created.VeiculoId.Should().NotBeEmpty();
            _ = created.DescricaoProblema.Should().Be("Cliente informou falha ao ligar pela manha.");
            _ = created.Status.Should().Be(1);
            _ = created.DataCriacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });

        var acompanhamentoResponse = await _client.GetAsync($"/api/ordensservico/acompanhamento/{created!.Id}");
        var acompanhamento = await acompanhamentoResponse.Content.ReadFromJsonAsync<RecuperarAcompanhamentoOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = acompanhamentoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = acompanhamento.Should().NotBeNull();
            _ = acompanhamento!.ClienteId.Should().Be(created.ClienteId);
            _ = acompanhamento.ClienteNome.Should().Be("Cliente OS Completa");
            _ = acompanhamento.VeiculoId.Should().Be(created.VeiculoId);
            _ = acompanhamento.VeiculoMarca.Should().Be("Honda");
            _ = acompanhamento.VeiculoModelo.Should().Be("Fit");
            _ = acompanhamento.VeiculoPlaca.Should().Be(GenerateValidPlaca(71));
            _ = acompanhamento.VeiculoAno.Should().Be(2021);
        });
    }

    [Test]
    public async Task Create_ShouldReturnNotFound_WhenClienteDoesNotExist()
    {
        var request = new
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Troca de oleo."
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Cliente");
            _ = error.Error.Should().Contain("encontrado");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Create_ShouldReturnNotFound_WhenVeiculoDoesNotExist()
    {
        var cliente = await CreateClientAsync(9002);
        var request = new
        {
            ClienteId = cliente.Id,
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Falha no sistema de freio."
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Veiculo");
            _ = error.Error.Should().Contain("encontrado");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenVeiculoDoesNotBelongToCliente()
    {
        var cliente1 = await CreateClientAsync(9003);
        var cliente2 = await CreateClientAsync(9004);
        var veiculoCliente2 = await CreateVehicleAsync(cliente2.Id, GenerateValidPlaca(32), "Honda", "Civic", 2023);

        var request = new
        {
            ClienteId = cliente1.Id,
            VeiculoId = veiculoCliente2.Id,
            DescricaoProblema = "Revisao completa."
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("O veiculo informado nao pertence ao cliente informado.");
            _ = error.ErrorCode.Should().Be("BadRequest");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDescricaoProblemaIsInvalid()
    {
        var cliente = await CreateClientAsync(9005);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(33), "Ford", "Ka", 2021);

        var request = new
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            DescricaoProblema = " "
        };

        var response = await _client.PostAsJsonAsync("/api/ordensservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Contains("descri", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        });
    }

    [Test]
    public async Task AddServico_ShouldSucceed_WhenRequestIsValid()
    {
        var cliente = await CreateClientAsync(9006);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(34), "Chevrolet", "Onix", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Troca de bateria");
        var servico = await CreateServicoAsync("Instalacao de bateria", 320m);
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });

        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new
        {
            ServicoId = servico.Id,
            Quantidade = 2
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", request);
        var created = await response.Content.ReadFromJsonAsync<ServicoDaOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.OrdemServicoId.Should().Be(ordemServico.Id);
            _ = created.ServicoId.Should().Be(servico.Id);
            _ = created.Descricao.Should().Be("Instalacao de bateria");
            _ = created.ValorUnitario.Should().Be(320m);
            _ = created.Quantidade.Should().Be(2);
            _ = created.ValorTotal.Should().Be(640m);
            _ = created.TempoGastoMinutos.Should().BeNull();
            _ = created.Concluido.Should().BeFalse();
        });
    }

    [Test]
    public async Task AddServico_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var servico = await CreateServicoAsync("Higienizacao do ar", 180m);

        var request = new
        {
            ServicoId = servico.Id,
            Quantidade = 1
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/addservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task AddServico_ShouldReturnBadRequest_WhenQuantidadeIsInvalid()
    {
        var cliente = await CreateClientAsync(9007);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(35), "Volkswagen", "Polo", 2023);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Revisao de 10.000 km");
        var servico = await CreateServicoAsync("Troca de filtro de oleo", 70m);
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });

        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new
        {
            ServicoId = servico.Id,
            Quantidade = 0
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("quantidade");
        });
    }

    [Test]
    public async Task AddServico_ShouldReturnBadRequest_WhenOrdemServicoIsNotEmDiagnostico()
    {
        var cliente = await CreateClientAsync(9008);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(36), "Renault", "Kwid", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Ruido na direcao");
        var servico = await CreateServicoAsync("Alinhamento", 150m);

        var request = new
        {
            ServicoId = servico.Id,
            Quantidade = 1
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("diagnostico");
        });
    }

    [Test]
    public async Task Get_ShouldListOrdensServicoAndFilterByClienteVeiculoAndStatus()
    {
        var cliente1 = await CreateClientAsync(9014);
        var cliente2 = await CreateClientAsync(9015);

        var veiculo1 = await CreateVehicleAsync(cliente1.Id, GenerateValidPlaca(42), "Toyota", "Corolla", 2023);
        var veiculo2 = await CreateVehicleAsync(cliente1.Id, GenerateValidPlaca(43), "Honda", "City", 2024);
        var veiculo3 = await CreateVehicleAsync(cliente2.Id, GenerateValidPlaca(44), "Ford", "Focus", 2022);

        var ordemRecebida = await CreateOrdemServicoAsync(cliente1.Id, veiculo1.Id, "Troca de amortecedor");
        var ordemEmDiagnostico = await CreateOrdemServicoAsync(cliente1.Id, veiculo2.Id, "Ruido na suspensao traseira");
        var ordemOutroCliente = await CreateOrdemServicoAsync(cliente2.Id, veiculo3.Id, "Vazamento de oleo");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemEmDiagnostico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAllResponse = await _client.GetAsync("/api/ordensservico?pageNumber=1&pageSize=10");
        var allResult = await getAllResponse.Content.ReadFromJsonAsync<ListarOrdensServicoResponse>();

        var getByClienteResponse = await _client.GetAsync($"/api/ordensservico?clienteId={cliente1.Id}&pageNumber=1&pageSize=10");
        var byCliente = await getByClienteResponse.Content.ReadFromJsonAsync<ListarOrdensServicoResponse>();

        var getByVeiculoResponse = await _client.GetAsync($"/api/ordensservico?veiculoId={veiculo1.Id}&pageNumber=1&pageSize=10");
        var byVeiculo = await getByVeiculoResponse.Content.ReadFromJsonAsync<ListarOrdensServicoResponse>();

        var getByStatusResponse = await _client.GetAsync("/api/ordensservico?status=2&pageNumber=1&pageSize=10");
        var byStatus = await getByStatusResponse.Content.ReadFromJsonAsync<ListarOrdensServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = allResult.Should().NotBeNull();
            _ = allResult!.OrdensServico.Count.Should().Be(3);
            _ = allResult.OrdensServico.Any(x => x.Id == ordemRecebida.Id).Should().BeTrue();
            _ = allResult.OrdensServico.Any(x => x.Id == ordemEmDiagnostico.Id).Should().BeTrue();
            _ = allResult.OrdensServico.Any(x => x.Id == ordemOutroCliente.Id).Should().BeTrue();

            _ = getByClienteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byCliente.Should().NotBeNull();
            _ = byCliente!.OrdensServico.Count.Should().Be(2);
            _ = byCliente.OrdensServico.All(x => x.ClienteId == cliente1.Id).Should().BeTrue();

            _ = getByVeiculoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byVeiculo.Should().NotBeNull();
            _ = byVeiculo!.OrdensServico.Count.Should().Be(1);
            _ = byVeiculo.OrdensServico.First().Id.Should().Be(ordemRecebida.Id);
            _ = byVeiculo.OrdensServico.First().VeiculoId.Should().Be(veiculo1.Id);

            _ = getByStatusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byStatus.Should().NotBeNull();
            _ = byStatus!.OrdensServico.Count.Should().Be(1);
            _ = byStatus.OrdensServico.First().Id.Should().Be(ordemEmDiagnostico.Id);
            _ = byStatus.OrdensServico.First().Status.Should().Be(2);
        });
    }

    [Test]
    public async Task Get_ShouldOrderByStatusAscAndDataAberturaDesc_WhenSortsAreInformed()
    {
        var cliente = await CreateClientAsync(9041);

        var veiculo1 = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(91), "Toyota", "Etios", 2021);
        var veiculo2 = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(92), "Honda", "HR-V", 2022);
        var veiculo3 = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(93), "Fiat", "Pulse", 2023);

        var ordemRecebidaAntiga = await CreateOrdemServicoAsync(cliente.Id, veiculo1.Id, "Primeira ordem recebida");
        await Task.Delay(20);
        var ordemRecebidaRecente = await CreateOrdemServicoAsync(cliente.Id, veiculo2.Id, "Segunda ordem recebida");
        await Task.Delay(20);
        var ordemEmDiagnostico = await CreateOrdemServicoAsync(cliente.Id, veiculo3.Id, "Ordem em diagnostico");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemEmDiagnostico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync("/api/ordensservico?statusSortDirection=Asc&dataAberturaSortDirection=Desc&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<ListarOrdensServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = result.Should().NotBeNull();
            _ = result!.OrdensServico.Select(x => x.Id).Should().Equal(
                ordemRecebidaRecente.Id,
                ordemRecebidaAntiga.Id,
                ordemEmDiagnostico.Id);
        });
    }

    [Test]
    public async Task Get_ShouldFilterByMultipleStatusAndKeepOrdering_WhenMultipleStatusAreInformed()
    {
        var cliente = await CreateClientAsync(9042);

        var veiculo1 = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(94), "Toyota", "Etios", 2021);
        var veiculo2 = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(95), "Honda", "HR-V", 2022);
        var veiculo3 = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(96), "Fiat", "Pulse", 2023);

        var ordemAguardandoAprovacao = await CreateOrdemServicoAsync(cliente.Id, veiculo1.Id, "Ordem aguardando aprovacao");
        await MoveToAguardandoAprovacaoAsync(ordemAguardandoAprovacao.Id);
        await Task.Delay(20);

        var ordemEmExecucao = await CreateOrdemServicoAsync(cliente.Id, veiculo2.Id, "Ordem em execucao");
        await MoveToAguardandoAprovacaoAsync(ordemEmExecucao.Id);
        var aprovarExecucaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemEmExecucao.Id}/aprovar-execucao", new { });
        _ = aprovarExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await Task.Delay(20);

        var ordemRecebida = await CreateOrdemServicoAsync(cliente.Id, veiculo3.Id, "Ordem recebida fora do filtro");

        var response = await _client.GetAsync("/api/ordensservico?status=3&status=4&statusSortDirection=Asc&dataAberturaSortDirection=Desc&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<ListarOrdensServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = result.Should().NotBeNull();
            _ = result!.OrdensServico.Select(x => x.Id).Should().Equal(ordemAguardandoAprovacao.Id, ordemEmExecucao.Id);
            _ = result.OrdensServico.Any(x => x.Id == ordemRecebida.Id).Should().BeFalse();
            _ = result.OrdensServico.Select(x => x.Status).Should().Equal(3, 4);
        });
    }

    [Test]
    public async Task GetById_ShouldReturnOrdemServicoComServicosEPecasInsumos()
    {
        var cliente = await CreateClientAsync(9016);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(45), "Chevrolet", "Tracker", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Ruido na suspensao dianteira");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var servico = await CreateServicoAsync("Troca de amortecedor", 480m);
        var addServicoResponse = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", new
        {
            ServicoId = servico.Id,
            Quantidade = 1
        });
        _ = addServicoResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var pecaInsumo = await CreatePecaInsumoAsync("Amortecedor dianteiro", "amt-os-450", "Conjunto completo", 390m, 20);
        var addPecaInsumoResponse = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 2
        });
        _ = addPecaInsumoResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var result = await response.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = result.Should().NotBeNull();
            _ = result!.Id.Should().Be(ordemServico.Id);
            _ = result.ClienteId.Should().Be(cliente.Id);
            _ = result.VeiculoId.Should().Be(veiculo.Id);
            _ = result.Status.Should().Be(2);
            _ = result.Servicos.Count.Should().Be(1);
            _ = result.PecasInsumos.Count.Should().Be(1);
            _ = result.Servicos.First().Descricao.Should().Be("Troca de amortecedor");
            _ = result.Servicos.First().ValorTotal.Should().Be(480m);
            _ = result.Servicos.First().Concluido.Should().BeFalse();
            _ = result.Servicos.First().TempoGastoMinutos.Should().BeNull();
            _ = result.PecasInsumos.First().Codigo.Should().Be("AMT-OS-450");
            _ = result.PecasInsumos.First().ValorTotal.Should().Be(780m);
            _ = result.ValorTotalServicos.Should().Be(480m);
            _ = result.ValorTotalPecasInsumos.Should().Be(780m);
            _ = result.ValorTotalOrdemServico.Should().Be(1260m);
        });
    }

    [Test]
    public async Task Delete_ShouldSoftDeleteOrdemServico_WhenOrdemServicoExists()
    {
        var cliente = await CreateClientAsync(9032);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(62), "Toyota", "Yaris", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Ordem para exclusao");

        var deleteResponse = await _client.DeleteAsync($"/api/ordensservico/{ordemServico.Id}");
        var getResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");

        Assert.Multiple(() =>
        {
            _ = deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _ = getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    [Test]
    public async Task GetAcompanhamentoById_ShouldReturnOrdemServico_WhenTokenIsMissing()
    {
        const int seed = 9031;
        var cliente = await CreateClientAsync(seed);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(61), "Fiat", "Pulse", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Acompanhamento publico da OS");

        var response = await _unauthorizedClient.GetAsync($"/api/ordensservico/acompanhamento/{ordemServico.Id}");
        var result = await response.Content.ReadFromJsonAsync<RecuperarAcompanhamentoOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = result.Should().NotBeNull();
            _ = result!.Id.Should().Be(ordemServico.Id);
            _ = result.ClienteId.Should().Be(cliente.Id);
            _ = result.ClienteNome.Should().Be($"Cliente OS {seed}");
            _ = result.VeiculoId.Should().Be(veiculo.Id);
            _ = result.VeiculoMarca.Should().Be("Fiat");
            _ = result.VeiculoModelo.Should().Be("Pulse");
            _ = result.VeiculoPlaca.Should().Be(GenerateValidPlaca(61));
            _ = result.VeiculoAno.Should().Be(2025);
            _ = result.Status.Should().Be(1);
            _ = result.DescricaoProblema.Should().Be("Acompanhamento publico da OS");
        });
    }

    [Test]
    public async Task GetStatusById_ShouldReturnOnlyIdAndStatus_WhenOrdemServicoExists()
    {
        var cliente = await CreateClientAsync(9033);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(63), "Jeep", "Renegade", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Consulta de status da OS");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}/status");
        var json = await response.Content.ReadAsStringAsync();
        using var payload = JsonDocument.Parse(json);
        var properties = payload.RootElement.EnumerateObject().Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = properties.Should().BeEquivalentTo(["id", "status"]);
            _ = properties.Length.Should().Be(2);
            _ = payload.RootElement.GetProperty("id").GetGuid().Should().Be(ordemServico.Id);
            _ = payload.RootElement.GetProperty("status").GetInt32().Should().Be(2);
        });
    }

    [Test]
    public async Task GetStatusById_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/ordensservico/{Guid.NewGuid()}/status");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task FluxoCompleto_ShouldSucceed_FromRecebidaToEntregue()
    {
        var cliente = await CreateClientAsync(9030);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(60), "Toyota", "Corolla", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Revisao geral com troca de componentes");

        var servico1 = await CreateServicoAsync("Troca de oleo", 350m);
        var servico2 = await CreateServicoAsync("Balanceamento", 120m);

        var pecaInsumo1 = await CreatePecaInsumoAsync("Filtro de oleo", "flt-os-901", "Filtro premium", 260m, 30);
        var pecaInsumo2 = await CreatePecaInsumoAsync("Aditivo de combustivel", "adt-os-902", "Aditivo para limpeza", 35m, 60);

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        var iniciarDiagnosticoBody = await iniciarDiagnosticoResponse.Content.ReadFromJsonAsync<IniciarDiagnosticoResponse>();

        var getEmDiagnosticoResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var getEmDiagnosticoBody = await getEmDiagnosticoResponse.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        var addServico1Response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", new
        {
            ServicoId = servico1.Id,
            Quantidade = 1
        });

        var addServico2Response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", new
        {
            ServicoId = servico2.Id,
            Quantidade = 2
        });
        var addServico1Body = await addServico1Response.Content.ReadFromJsonAsync<ServicoDaOrdemServicoResponse>();
        var addServico2Body = await addServico2Response.Content.ReadFromJsonAsync<ServicoDaOrdemServicoResponse>();

        var addPecaInsumo1Response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", new
        {
            PecaInsumoId = pecaInsumo1.Id,
            Quantidade = 2
        });

        var addPecaInsumo2Response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", new
        {
            PecaInsumoId = pecaInsumo2.Id,
            Quantidade = 3
        });

        var getComItensResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var getComItensBody = await getComItensResponse.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        var solicitarAprovacaoBody = await solicitarAprovacaoResponse.Content.ReadFromJsonAsync<SolicitarAprovacaoResponse>();

        var getAguardandoAprovacaoResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var getAguardandoAprovacaoBody = await getAguardandoAprovacaoResponse.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        var aprovarExecucaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { });
        var aprovarExecucaoBody = await aprovarExecucaoResponse.Content.ReadFromJsonAsync<AprovarExecucaoResponse>();

        var getEmExecucaoResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var getEmExecucaoBody = await getEmExecucaoResponse.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        var concluirServico1Response = await _client.PutAsJsonAsync($"/api/ordensservico/servicos/{addServico1Body!.Id}/concluir", new
        {
            TempoGastoMinutos = 80
        });
        var concluirServico1Body = await concluirServico1Response.Content.ReadFromJsonAsync<ConcluirServicoOrdemServicoResponse>();

        var concluirServico2Response = await _client.PutAsJsonAsync($"/api/ordensservico/servicos/{addServico2Body!.Id}/concluir", new
        {
            TempoGastoMinutos = 45
        });
        var concluirServico2Body = await concluirServico2Response.Content.ReadFromJsonAsync<ConcluirServicoOrdemServicoResponse>();

        var finalizarResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/finalizar", new { });
        var finalizarBody = await finalizarResponse.Content.ReadFromJsonAsync<FinalizarOrdemServicoResponse>();

        var getConcluidaResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var getConcluidaBody = await getConcluidaResponse.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        var entregarResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/entregar", new { });
        var entregarBody = await entregarResponse.Content.ReadFromJsonAsync<EntregarOrdemServicoResponse>();

        var getEntregueResponse = await _client.GetAsync($"/api/ordensservico/{ordemServico.Id}");
        var getEntregueBody = await getEntregueResponse.Content.ReadFromJsonAsync<RecuperarOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = iniciarDiagnosticoBody.Should().NotBeNull();
            _ = iniciarDiagnosticoBody!.Status.Should().Be(2);

            _ = getEmDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = getEmDiagnosticoBody.Should().NotBeNull();
            _ = getEmDiagnosticoBody!.Status.Should().Be(2);
            _ = getEmDiagnosticoBody.Servicos.Count.Should().Be(0);
            _ = getEmDiagnosticoBody.PecasInsumos.Count.Should().Be(0);

            _ = addServico1Response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = addServico2Response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = addServico1Body.Should().NotBeNull();
            _ = addServico2Body.Should().NotBeNull();
            _ = addPecaInsumo1Response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = addPecaInsumo2Response.StatusCode.Should().Be(HttpStatusCode.Created);

            _ = getComItensResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = getComItensBody.Should().NotBeNull();
            _ = getComItensBody!.Status.Should().Be(2);
            _ = getComItensBody.Servicos.Count.Should().Be(2);
            _ = getComItensBody.PecasInsumos.Count.Should().Be(2);
            _ = getComItensBody.ValorTotalServicos.Should().Be(590m);
            _ = getComItensBody.ValorTotalPecasInsumos.Should().Be(625m);
            _ = getComItensBody.ValorTotalOrdemServico.Should().Be(1215m);

            _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = solicitarAprovacaoBody.Should().NotBeNull();
            _ = solicitarAprovacaoBody!.Status.Should().Be(3);

            _ = getAguardandoAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = getAguardandoAprovacaoBody.Should().NotBeNull();
            _ = getAguardandoAprovacaoBody!.Status.Should().Be(3);

            _ = aprovarExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = aprovarExecucaoBody.Should().NotBeNull();
            _ = aprovarExecucaoBody!.Status.Should().Be(4);

            _ = getEmExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = getEmExecucaoBody.Should().NotBeNull();
            _ = getEmExecucaoBody!.Status.Should().Be(4);

            _ = concluirServico1Response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = concluirServico1Body.Should().NotBeNull();
            _ = concluirServico1Body!.Concluido.Should().BeTrue();
            _ = concluirServico1Body.TempoGastoMinutos.Should().Be(80);

            _ = concluirServico2Response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = concluirServico2Body.Should().NotBeNull();
            _ = concluirServico2Body!.Concluido.Should().BeTrue();
            _ = concluirServico2Body.TempoGastoMinutos.Should().Be(45);

            _ = finalizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = finalizarBody.Should().NotBeNull();
            _ = finalizarBody!.Status.Should().Be(5);

            _ = getConcluidaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = getConcluidaBody.Should().NotBeNull();
            _ = getConcluidaBody!.Status.Should().Be(5);

            _ = entregarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = entregarBody.Should().NotBeNull();
            _ = entregarBody!.Status.Should().Be(6);

            _ = getEntregueResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = getEntregueBody.Should().NotBeNull();
            _ = getEntregueBody!.Status.Should().Be(6);
            _ = getEntregueBody.Servicos.Count.Should().Be(2);
            _ = getEntregueBody.Servicos.All(x => x.Concluido).Should().BeTrue();
            _ = getEntregueBody.PecasInsumos.Count.Should().Be(2);
            _ = getEntregueBody.ValorTotalServicos.Should().Be(590m);
            _ = getEntregueBody.ValorTotalPecasInsumos.Should().Be(625m);
            _ = getEntregueBody.ValorTotalOrdemServico.Should().Be(1215m);
        });
    }

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/ordensservico/{Guid.NewGuid()}");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Get_ShouldReturnBadRequest_WhenPaginationIsInvalid()
    {
        var response = await _client.GetAsync("/api/ordensservico?pageNumber=0&pageSize=10");
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
    public async Task AddPecaInsumo_ShouldSucceed_WhenRequestIsValid()
    {
        var cliente = await CreateClientAsync(9011);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(39), "Nissan", "Versa", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Troca de pastilhas dianteiras");
        var pecaInsumo = await CreatePecaInsumoAsync("Pastilha de freio", "pst-os-001", "Pastilha dianteira", 210m, 15);
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });

        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 3
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", request);
        var created = await response.Content.ReadFromJsonAsync<PecaInsumoDaOrdemServicoResponse>();
        var pecaInsumoAtualizadaResponse = await _client.GetAsync($"/api/pecasinsumos/{pecaInsumo.Id}");
        var pecaInsumoAtualizada = await pecaInsumoAtualizadaResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.OrdemServicoId.Should().Be(ordemServico.Id);
            _ = created.PecaInsumoId.Should().Be(pecaInsumo.Id);
            _ = created.Nome.Should().Be("Pastilha de freio");
            _ = created.Codigo.Should().Be("PST-OS-001");
            _ = created.Descricao.Should().Be("Pastilha dianteira");
            _ = created.PrecoUnitario.Should().Be(210m);
            _ = created.Quantidade.Should().Be(3);
            _ = created.ValorTotal.Should().Be(630m);
            _ = pecaInsumoAtualizadaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = pecaInsumoAtualizada.Should().NotBeNull();
            _ = pecaInsumoAtualizada!.QuantidadeEstoque.Should().Be(12);
        });
    }

    [Test]
    public async Task AddPecaInsumo_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var pecaInsumo = await CreatePecaInsumoAsync("Disco de freio", "dsc-os-100", "Disco ventilado", 350m, 10);

        var request = new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 1
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/addpecainsumo", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task AddPecaInsumo_ShouldReturnBadRequest_WhenQuantidadeIsInvalid()
    {
        var cliente = await CreateClientAsync(9012);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(40), "Jeep", "Compass", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Revisao de freios");
        var pecaInsumo = await CreatePecaInsumoAsync("Fluido de freio", "fld-os-200", "Fluido DOT4", 59m, 40);
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });

        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 0
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("quantidade");
        });
    }

    [Test]
    public async Task AddPecaInsumo_ShouldReturnBadRequest_WhenQuantidadeExceedsEstoqueDisponivel()
    {
        var cliente = await CreateClientAsync(90121);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(401), "Renault", "Kwid", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Substituicao de componente");
        var pecaInsumo = await CreatePecaInsumoAsync("Sensor ABS", "sns-os-210", "Sensor dianteiro", 125m, 1);
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });

        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 2
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("estoque");
            _ = error.ErrorCode.Should().Be("BadRequest");
        });
    }

    [Test]
    public async Task AddPecaInsumo_ShouldReturnBadRequest_WhenOrdemServicoIsNotEmDiagnostico()
    {
        var cliente = await CreateClientAsync(9013);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(41), "Peugeot", "208", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Troca de filtro");
        var pecaInsumo = await CreatePecaInsumoAsync("Filtro de ar", "flt-os-300", "Filtro do motor", 45m, 25);

        var request = new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 1
        };

        var response = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("diagnostico");
        });
    }

    [Test]
    public async Task IniciarDiagnostico_ShouldSucceed_WhenOrdemServicoIsRecebida()
    {
        var cliente = await CreateClientAsync(9009);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(37), "Hyundai", "HB20", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha intermitente no motor");

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        var updated = await response.Content.ReadFromJsonAsync<IniciarDiagnosticoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(ordemServico.Id);
            _ = updated.Status.Should().Be(2);
            _ = updated.DataInicioDiagnostico.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });
    }

    [Test]
    public async Task IniciarDiagnostico_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/iniciar-diagnostico", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task IniciarDiagnostico_ShouldReturnBadRequest_WhenOrdemServicoIsNotRecebida()
    {
        var cliente = await CreateClientAsync(9010);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(38), "Fiat", "Argo", 2023);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Cheiro de queimado");

        var primeiraTentativaResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });

        _ = primeiraTentativaResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var segundaTentativaResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        var error = await segundaTentativaResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = segundaTentativaResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("recebidas");
        });
    }

    [Test]
    public async Task SolicitarAprovacao_ShouldSucceed_WhenOrdemServicoIsEmDiagnostico()
    {
        var cliente = await CreateClientAsync(9017);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(46), "Toyota", "Corolla", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha no sistema eletrico");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        var updated = await response.Content.ReadFromJsonAsync<SolicitarAprovacaoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(ordemServico.Id);
            _ = updated.Status.Should().Be(3);
            _ = updated.DataEnvioAprovacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });
    }

    [Test]
    public async Task SolicitarAprovacao_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/solicitar-aprovacao", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task SolicitarAprovacao_ShouldReturnBadRequest_WhenOrdemServicoIsNotEmDiagnostico()
    {
        var cliente = await CreateClientAsync(9018);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(47), "Honda", "Civic", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha no alternador");

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("diagnóstico");
        });
    }

    [Test]
    public async Task AprovarExecucao_ShouldSucceed_WhenOrdemServicoIsAguardandoAprovacao()
    {
        var cliente = await CreateClientAsync(9019);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(48), "Jeep", "Renegade", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Luz de injecao acesa");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { });
        var updated = await response.Content.ReadFromJsonAsync<AprovarExecucaoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(ordemServico.Id);
            _ = updated.Status.Should().Be(4);
            _ = updated.DataInicioExecucao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });
    }

    [Test]
    public async Task AprovarExecucao_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/aprovar-execucao", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task AprovarExecucao_ShouldReturnBadRequest_WhenOrdemServicoIsNotAguardandoAprovacao()
    {
        var cliente = await CreateClientAsync(9020);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(49), "Ford", "Territory", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Vibracao no volante");

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("aguardando aprovação");
        });
    }

    [Test]
    public async Task Cancelar_ShouldSucceed_AndReturnReservedStock_WhenOrdemServicoIsAguardandoAprovacao()
    {
        var cliente = await CreateClientAsync(9040);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(70), "Honda", "Fit", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Ruido no freio dianteiro");
        var pecaInsumo = await CreatePecaInsumoAsync("Pastilha de freio dianteira", "pst-os-9040", "Jogo dianteiro", 210m, 12);

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var addPecaInsumoResponse = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addpecainsumo", new
        {
            PecaInsumoId = pecaInsumo.Id,
            Quantidade = 4
        });
        _ = addPecaInsumoResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var pecaReservadaResponse = await _client.GetAsync($"/api/pecasinsumos/{pecaInsumo.Id}");
        var pecaReservada = await pecaReservadaResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/cancelar", new { });
        var updated = await response.Content.ReadFromJsonAsync<CancelarOrdemServicoResponse>();

        var pecaDevolvidaResponse = await _client.GetAsync($"/api/pecasinsumos/{pecaInsumo.Id}");
        var pecaDevolvida = await pecaDevolvidaResponse.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        Assert.Multiple(() =>
        {
            _ = pecaReservadaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = pecaReservada.Should().NotBeNull();
            _ = pecaReservada!.QuantidadeEstoque.Should().Be(8);

            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(ordemServico.Id);
            _ = updated.Status.Should().Be(7);

            _ = pecaDevolvidaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = pecaDevolvida.Should().NotBeNull();
            _ = pecaDevolvida!.QuantidadeEstoque.Should().Be(12);
        });
    }

    [Test]
    public async Task Cancelar_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/cancelar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Cancelar_ShouldReturnBadRequest_WhenOrdemServicoIsNotAguardandoAprovacao()
    {
        var cliente = await CreateClientAsync(9041);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(71), "Fiat", "Cronos", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha no sensor de temperatura");

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/cancelar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("aguardando aprovação");
        });
    }

    [Test]
    public async Task ConcluirServico_ShouldSucceed_WhenRequestIsValid()
    {
        var cliente = await CreateClientAsync(9026);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(54), "Jeep", "Compass", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha intermitente no motor");
        var servico = await CreateServicoAsync("Diagnostico eletrico", 280m);

        _ = (await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        var addServicoResponse = await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", new { ServicoId = servico.Id, Quantidade = 1 });
        var addServicoBody = await addServicoResponse.Content.ReadFromJsonAsync<ServicoDaOrdemServicoResponse>();
        _ = addServicoResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _ = (await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        _ = (await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { })).StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/servicos/{addServicoBody!.Id}/concluir", new { TempoGastoMinutos = 65 });
        var updated = await response.Content.ReadFromJsonAsync<ConcluirServicoOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(addServicoBody.Id);
            _ = updated.Concluido.Should().BeTrue();
            _ = updated.TempoGastoMinutos.Should().Be(65);
        });
    }

    [Test]
    public async Task Finalizar_ShouldReturnBadRequest_WhenExisteServicoNaoConcluido()
    {
        var cliente = await CreateClientAsync(9027);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(55), "Fiat", "Pulse", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha no sistema de freio");
        var servico = await CreateServicoAsync("Troca de pastilhas", 220m);

        _ = (await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        _ = (await _client.PostAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/addservico", new { ServicoId = servico.Id, Quantidade = 1 })).StatusCode.Should().Be(HttpStatusCode.Created);
        _ = (await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        _ = (await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { })).StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/finalizar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("todos os serviços concluídos");
        });
    }

    [Test]
    public async Task Finalizar_ShouldSucceed_WhenOrdemServicoIsEmExecucao()
    {
        var cliente = await CreateClientAsync(9021);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(50), "Volkswagen", "T-Cross", 2025);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Barulho no eixo traseiro");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var aprovarExecucaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { });
        _ = aprovarExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/finalizar", new { });
        var updated = await response.Content.ReadFromJsonAsync<FinalizarOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(ordemServico.Id);
            _ = updated.Status.Should().Be(5);
            _ = updated.DataFinalizacao.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });
    }

    [Test]
    public async Task Finalizar_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/finalizar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Finalizar_ShouldReturnBadRequest_WhenOrdemServicoIsNotEmExecucao()
    {
        var cliente = await CreateClientAsync(9022);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(51), "Nissan", "Kicks", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Ruido na coluna de direcao");

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/finalizar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("execução");
        });
    }

    [Test]
    public async Task Entregar_ShouldSucceed_WhenOrdemServicoIsFinalizada()
    {
        var cliente = await CreateClientAsync(9023);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(52), "Hyundai", "HB20", 2024);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Falha no sistema de arrefecimento");

        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/solicitar-aprovacao", new { });
        _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var aprovarExecucaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/aprovar-execucao", new { });
        _ = aprovarExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalizarResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/finalizar", new { });
        _ = finalizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/entregar", new { });
        var updated = await response.Content.ReadFromJsonAsync<EntregarOrdemServicoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(ordemServico.Id);
            _ = updated.Status.Should().Be(6);
            _ = updated.DataEntrega.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
        });
    }

    [Test]
    public async Task Entregar_ShouldReturnNotFound_WhenOrdemServicoDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{Guid.NewGuid()}/entregar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("Ordem");
            _ = error.Error.Should().Contain("encontrada");
            _ = error.ErrorCode.Should().Be("NotFound");
        });
    }

    [Test]
    public async Task Entregar_ShouldReturnBadRequest_WhenOrdemServicoIsNotFinalizada()
    {
        var cliente = await CreateClientAsync(9024);
        var veiculo = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(53), "Kia", "Cerato", 2023);
        var ordemServico = await CreateOrdemServicoAsync(cliente.Id, veiculo.Id, "Barulho na correia");

        var response = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServico.Id}/entregar", new { });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Contain("finalizadas");
        });
    }

    [Test]
    public async Task AllActions_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        var ordemServicoId = Guid.NewGuid();
        var servicoDaOrdemServicoId = Guid.NewGuid();

        var getResponse = await SendUnauthorizedAsync(HttpMethod.Get, "/api/ordensservico");
        var getByIdResponse = await SendUnauthorizedAsync(HttpMethod.Get, $"/api/ordensservico/{ordemServicoId}");
        var getStatusByIdResponse = await SendUnauthorizedAsync(HttpMethod.Get, $"/api/ordensservico/{ordemServicoId}/status");
        var postResponse = await SendUnauthorizedAsync(HttpMethod.Post, "/api/ordensservico", new
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Teste sem token"
        });
        var postComClienteEVeiculoResponse = await SendUnauthorizedAsync(HttpMethod.Post, "/api/ordensservico/com-cliente-veiculo", new
        {
            Cliente = new
            {
                Nome = "Cliente sem token",
                Cpf = GenerateValidCpf(9040),
                Telefone = "11989040000"
            },
            Veiculo = new
            {
                Placa = GenerateValidPlaca(90),
                Marca = "Fiat",
                Modelo = "Argo",
                Ano = 2023
            },
            DescricaoProblema = "Teste sem token"
        });
        var addServicoResponse = await SendUnauthorizedAsync(HttpMethod.Post, $"/api/ordensservico/{ordemServicoId}/addservico", new
        {
            ServicoId = Guid.NewGuid(),
            Quantidade = 1
        });
        var addPecaInsumoResponse = await SendUnauthorizedAsync(HttpMethod.Post, $"/api/ordensservico/{ordemServicoId}/addpecainsumo", new
        {
            PecaInsumoId = Guid.NewGuid(),
            Quantidade = 1
        });
        var iniciarDiagnosticoResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/{ordemServicoId}/iniciar-diagnostico", new { });
        var solicitarAprovacaoResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/{ordemServicoId}/solicitar-aprovacao", new { });
        var aprovarExecucaoResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/{ordemServicoId}/aprovar-execucao", new { });
        var cancelarResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/{ordemServicoId}/cancelar", new { });
        var concluirServicoResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/servicos/{servicoDaOrdemServicoId}/concluir", new
        {
            TempoGastoMinutos = 30
        });
        var finalizarResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/{ordemServicoId}/finalizar", new { });
        var entregarResponse = await SendUnauthorizedAsync(HttpMethod.Put, $"/api/ordensservico/{ordemServicoId}/entregar", new { });
        var deleteResponse = await SendUnauthorizedAsync(HttpMethod.Delete, $"/api/ordensservico/{ordemServicoId}");

        Assert.Multiple(() =>
        {
            _ = getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = getStatusByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = postComClienteEVeiculoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = addServicoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = addPecaInsumoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = aprovarExecucaoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = cancelarResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = concluirServicoResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = finalizarResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _ = entregarResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

    private async Task<ClienteResponse> CreateClientAsync(int seed)
    {
        var request = new
        {
            Nome = $"Cliente OS {seed}",
            Cpf = GenerateValidCpf(seed),
            Telefone = $"1198{seed:D7}",
            Email = $"cliente.os{seed}@email.com"
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

    private async Task<VeiculoResponse> CreateVehicleAsync(Guid clienteId, string placa, string marca, string modelo, int ano)
    {
        var request = new
        {
            ClienteId = clienteId,
            Placa = placa,
            Marca = marca,
            Modelo = modelo,
            Ano = ano
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

    private async Task MoveToAguardandoAprovacaoAsync(Guid ordemServicoId)
    {
        var iniciarDiagnosticoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServicoId}/iniciar-diagnostico", new { });
        _ = iniciarDiagnosticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var solicitarAprovacaoResponse = await _client.PutAsJsonAsync($"/api/ordensservico/{ordemServicoId}/solicitar-aprovacao", new { });
        _ = solicitarAprovacaoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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

    private async Task<PecaInsumoResponse> CreatePecaInsumoAsync(string nome, string codigo, string descricao, decimal precoUnitario, int quantidadeEstoque)
    {
        var request = new
        {
            Nome = nome,
            Codigo = codigo,
            Descricao = descricao,
            PrecoUnitario = precoUnitario,
            QuantidadeEstoque = quantidadeEstoque
        };

        var response = await _client.PostAsJsonAsync("/api/pecasinsumos", request);
        var created = await response.Content.ReadFromJsonAsync<PecaInsumoResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
        });

        return created!;
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
        public Guid ClienteId { get; set; }
        public Guid VeiculoId { get; set; }
        public string DescricaoProblema { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    private sealed class ListarOrdensServicoResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public IReadOnlyCollection<ListarOrdemServicoItemResponse> OrdensServico { get; set; } = [];
    }

    private sealed class ListarOrdemServicoItemResponse
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid VeiculoId { get; set; }
        public int Status { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    private sealed class RecuperarOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public Guid VeiculoId { get; set; }
        public int Status { get; set; }
        public IReadOnlyCollection<RecuperarServicoDaOrdemServicoItemResponse> Servicos { get; set; } = [];
        public IReadOnlyCollection<RecuperarPecaInsumoDaOrdemServicoItemResponse> PecasInsumos { get; set; } = [];
        public decimal ValorTotalServicos { get; set; }
        public decimal ValorTotalPecasInsumos { get; set; }
        public decimal ValorTotalOrdemServico { get; set; }
    }

    private sealed class RecuperarAcompanhamentoOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public Guid VeiculoId { get; set; }
        public string VeiculoMarca { get; set; } = string.Empty;
        public string VeiculoModelo { get; set; } = string.Empty;
        public string VeiculoPlaca { get; set; } = string.Empty;
        public int VeiculoAno { get; set; }
        public string DescricaoProblema { get; set; } = string.Empty;
        public int Status { get; set; }
    }

    private sealed class RecuperarServicoDaOrdemServicoItemResponse
    {
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public int? TempoGastoMinutos { get; set; }
        public bool Concluido { get; set; }
    }

    private sealed class RecuperarPecaInsumoDaOrdemServicoItemResponse
    {
        public string Codigo { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
    }

    private sealed class ServicoResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class PecaInsumoResponse
    {
        public Guid Id { get; set; }
        public int QuantidadeEstoque { get; set; }
    }

    private sealed class ServicoDaOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public Guid OrdemServicoId { get; set; }
        public Guid ServicoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public int? TempoGastoMinutos { get; set; }
        public bool Concluido { get; set; }
    }

    private sealed class ConcluirServicoOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public Guid OrdemServicoId { get; set; }
        public Guid ServicoId { get; set; }
        public int TempoGastoMinutos { get; set; }
        public bool Concluido { get; set; }
    }

    private sealed class PecaInsumoDaOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public Guid OrdemServicoId { get; set; }
        public Guid PecaInsumoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
    }

    private sealed class IniciarDiagnosticoResponse
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DataInicioDiagnostico { get; set; }
    }

    private sealed class SolicitarAprovacaoResponse
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DataEnvioAprovacao { get; set; }
    }

    private sealed class AprovarExecucaoResponse
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DataInicioExecucao { get; set; }
    }

    private sealed class CancelarOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
    }

    private sealed class FinalizarOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DataFinalizacao { get; set; }
    }

    private sealed class EntregarOrdemServicoResponse
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DataEntrega { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }
}

