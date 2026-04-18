using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class OrdensServicoControllerTests
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
    public async Task Get_ShouldReturnBadRequest_WhenPaginationIsInvalid()
    {
        var response = await _client.GetAsync("/api/ordensservico?pageNumber=0&pageSize=10");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("O numero da pagina deve ser maior que zero.");
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
    }

    private sealed class ServicoResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class PecaInsumoResponse
    {
        public Guid Id { get; set; }
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

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
