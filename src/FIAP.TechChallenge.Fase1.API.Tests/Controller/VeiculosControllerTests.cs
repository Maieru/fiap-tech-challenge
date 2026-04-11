using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class VeiculosControllerTests
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
        var cliente = await CreateClientAsync(7001);

        var createRequest = new
        {
            ClienteId = cliente.Id,
            Placa = GenerateValidPlaca(1),
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        };

        var postResponse = await _client.PostAsJsonAsync("/api/veiculos", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<VeiculoResponse>();

        Assert.Multiple(() =>
        {
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.ClienteId.Should().Be(cliente.Id);
            _ = created.Placa.Should().Be(createRequest.Placa);
            _ = created.Marca.Should().Be("Toyota");
            _ = created.Modelo.Should().Be("Corolla");
            _ = created.Ano.Should().Be(2022);
        });

        var updateRequest = new
        {
            Placa = GenerateValidPlaca(2),
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2023
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/veiculos/{created!.Id}", updateRequest);
        var updated = await putResponse.Content.ReadFromJsonAsync<VeiculoResponse>();

        Assert.Multiple(() =>
        {
            _ = putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.ClienteId.Should().Be(cliente.Id);
            _ = updated.Placa.Should().Be(updateRequest.Placa);
            _ = updated.Marca.Should().Be("Honda");
            _ = updated.Modelo.Should().Be("Civic");
            _ = updated.Ano.Should().Be(2023);
        });
    }

    [Test]
    public async Task Create_ShouldReturnNotFound_WhenClientDoesNotExist()
    {
        var request = new
        {
            ClienteId = Guid.NewGuid(),
            Placa = GenerateValidPlaca(3),
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2020
        };

        var response = await _client.PostAsJsonAsync("/api/veiculos", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Cliente não encontrado.");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenPlacaIsInvalid()
    {
        var cliente = await CreateClientAsync(7002);

        var request = new
        {
            ClienteId = cliente.Id,
            Placa = "ABC123",
            Marca = "Chevrolet",
            Modelo = "Onix",
            Ano = 2021
        };

        var response = await _client.PostAsJsonAsync("/api/veiculos", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("A placa deve ter exatamente 7 caracteres alfanuméricos.");
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenPlacaAlreadyExists()
    {
        var cliente = await CreateClientAsync(7003);
        var placa = GenerateValidPlaca(4);

        _ = await CreateVehicleAsync(cliente.Id, placa, "Volkswagen", "Gol", 2019);

        var duplicatedRequest = new
        {
            ClienteId = cliente.Id,
            Placa = placa,
            Marca = "Fiat",
            Modelo = "Argo",
            Ano = 2020
        };

        var response = await _client.PostAsJsonAsync("/api/veiculos", duplicatedRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Ja existe um veiculo cadastrado com esta placa.");
        });
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenVehicleDoesNotExist()
    {
        var request = new
        {
            Placa = GenerateValidPlaca(5),
            Marca = "Nissan",
            Modelo = "March",
            Ano = 2022
        };

        var response = await _client.PutAsJsonAsync($"/api/veiculos/{Guid.NewGuid()}", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Veículo não encontrado.");
        });
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenPlacaAlreadyExists()
    {
        var cliente = await CreateClientAsync(7004);

        var first = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(6), "Hyundai", "HB20", 2021);
        var second = await CreateVehicleAsync(cliente.Id, GenerateValidPlaca(7), "Renault", "Sandero", 2020);

        var request = new
        {
            Placa = first.Placa,
            Marca = "Renault",
            Modelo = "Sandero",
            Ano = 2020
        };

        var response = await _client.PutAsJsonAsync($"/api/veiculos/{second.Id}", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Ja existe um veiculo cadastrado com esta placa.");
        });
    }

    private async Task<ClienteResponse> CreateClientAsync(int seed)
    {
        var request = new
        {
            Nome = $"Cliente Veiculo {seed}",
            Cpf = GenerateValidCpf(seed),
            Telefone = $"1198{seed:D7}",
            Email = $"cliente.veiculo{seed}@email.com"
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
        });

        return created!;
    }

    private static string GenerateValidPlaca(int seed)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var first = alphabet[(seed / 676) % 26];
        var second = alphabet[(seed / 26) % 26];
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
        public Guid ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
