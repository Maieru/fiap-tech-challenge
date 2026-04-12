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

    private sealed class ServicoResponse
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

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
