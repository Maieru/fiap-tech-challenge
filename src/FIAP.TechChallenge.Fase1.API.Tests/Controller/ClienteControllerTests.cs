using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FIAP.TechChallenge.Fase1.API.Tests.Controller;

[TestFixture]
public sealed class ClienteControllerTests
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
    public async Task CreateGetByIdUpdateGetById_ShouldSucceed_WhenFlowIsValid()
    {
        var createRequest = new
        {
            Nome = "Maria Silva",
            Cpf = GenerateValidCpf(1),
            Telefone = "11999999999",
            Email = "maria@email.com"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/clientes", createRequest);
        _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.Multiple(() =>
        {
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
            _ = created.Nome.Should().Be("Maria Silva");
        });

        var getResponse = await _client.GetAsync($"/api/clientes?id={created.Id}");
        _ = getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cliente = await getResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.Multiple(() =>
        {
            _ = cliente.Should().NotBeNull();
            _ = cliente!.Id.Should().Be(created.Id);
            _ = cliente.Nome.Should().Be("Maria Silva");
            _ = cliente.Email.Should().Be("maria@email.com");
        });

        var updateRequest = new
        {
            Nome = "Maria Souza",
            Telefone = "11988888888",
            Email = "maria.souza@email.com"
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/clientes/{created.Id}", updateRequest);
        _ = putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getUpdatedResponse = await _client.GetAsync($"/api/clientes?id={created.Id}");
        _ = getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getUpdatedResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.Multiple(() =>
        {
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.Nome.Should().Be("Maria Souza");
            _ = updated.Telefone.Should().Be("(11) 98888-8888");
            _ = updated.Email.Should().Be("maria.souza@email.com");
        });
    }

    [Test]
    public async Task CreateGetByCpfUpdateGetById_ShouldReflectUpdatedData()
    {
        var createRequest = new
        {
            Nome = "João Silva",
            Cpf = GenerateValidCpf(3001),
            Telefone = "11977777777",
            Email = "joao@email.com"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/clientes", createRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
            _ = created!.Id.Should().NotBeEmpty();
        });

        var getByCpfResponse = await _client.GetAsync($"/api/clientes?cpf={created.Cpf}");
        var byCpf = await getByCpfResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = getByCpfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = byCpf.Should().NotBeNull();
            _ = byCpf!.Id.Should().Be(created.Id);
            _ = byCpf.Cpf.Should().Be(created.Cpf);
        });

        var updateRequest = new
        {
            Nome = "João Souza",
            Telefone = "11966666666",
            Email = "joao.souza@email.com"
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/clientes/{created.Id}", updateRequest);
        _ = putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getByIdResponse = await _client.GetAsync($"/api/clientes?id={created.Id}");
        var updated = await getByIdResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.Nome.Should().Be("João Souza");
            _ = updated.Telefone.Should().Be("(11) 96666-6666");
            _ = updated.Email.Should().Be("joao.souza@email.com");
        });
    }

    [Test]
    public async Task CreateUpdateGetByCpf_ShouldKeepIdentityAndReturnUpdatedFields()
    {
        var created = await CreateClientAsync(3002);

        var updateRequest = new
        {
            Nome = "Cliente Renomeado",
            Telefone = "11955555555",
            Email = "cliente.renomeado@email.com"
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/clientes/{created.Id}", updateRequest);
        _ = putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getByCpfResponse = await _client.GetAsync($"/api/clientes?cpf={created.Cpf}");
        var updated = await getByCpfResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = getByCpfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = updated.Should().NotBeNull();
            _ = updated!.Id.Should().Be(created.Id);
            _ = updated.Cpf.Should().Be(created.Cpf);
            _ = updated.Nome.Should().Be("Cliente Renomeado");
            _ = updated.Telefone.Should().Be("(11) 95555-5555");
            _ = updated.Email.Should().Be("cliente.renomeado@email.com");
        });
    }

    [Test]
    public async Task CreateManyGetPagedThenGetById_ShouldReturnConsistentClientData()
    {
        for (var i = 4001; i <= 4012; i++)
            _ = await CreateClientAsync(i);

        var pagedResponse = await _client.GetAsync("/api/clientes?pageNumber=2&pageSize=5");

        using var json = JsonDocument.Parse(await pagedResponse.Content.ReadAsStringAsync());
        var clientes = json.RootElement.GetProperty("clientes");

        var firstClient = clientes[0];
        var id = firstClient.GetProperty("id").GetGuid();
        var cpf = firstClient.GetProperty("cpf").GetString();
        var nome = firstClient.GetProperty("nome").GetString();

        var getByIdResponse = await _client.GetAsync($"/api/clientes?id={id}");
        var cliente = await getByIdResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = pagedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = clientes.GetArrayLength().Should().Be(5);
            _ = getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = cliente.Should().NotBeNull();
            _ = cliente!.Id.Should().Be(id);
            _ = cliente.Cpf.Should().Be(cpf);
            _ = cliente.Nome.Should().Be(nome);
        });
    }

    [Test]
    public async Task CreateThenGetWithMultipleFilters_ShouldReturnBadRequest_AndNotAffectSubsequentValidGet()
    {
        var created = await CreateClientAsync(5001);

        var invalidGetResponse = await _client.GetAsync($"/api/clientes?id={created.Id}&cpf={created.Cpf}");
        var error = await invalidGetResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        var validGetResponse = await _client.GetAsync($"/api/clientes?id={created.Id}");
        var cliente = await validGetResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = invalidGetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Informe apenas um filtro por vez: id, cpf ou cnpj.");
            _ = validGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = cliente.Should().NotBeNull();
            _ = cliente!.Id.Should().Be(created.Id);
            _ = cliente.Cpf.Should().Be(created.Cpf);
        });
    }

    [Test]
    public async Task CreateGetByIdUpdateNonExistingGetById_ShouldNotChangeOriginalClient()
    {
        var created = await CreateClientAsync(5002);

        var updateRequest = new
        {
            Nome = "Alteração inválida",
            Telefone = "11944444444",
            Email = "alteracao.invalida@email.com"
        };

        var invalidPutResponse = await _client.PutAsJsonAsync($"/api/clientes/{Guid.NewGuid()}", updateRequest);
        var error = await invalidPutResponse.Content.ReadFromJsonAsync<ErrorResponse>();

        var getOriginalResponse = await _client.GetAsync($"/api/clientes?id={created.Id}");
        var original = await getOriginalResponse.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = invalidPutResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Cliente não encontrado.");
            _ = getOriginalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = original.Should().NotBeNull();
            _ = original!.Id.Should().Be(created.Id);
            _ = original.Nome.Should().Be(created.Nome);
            _ = original.Cpf.Should().Be(created.Cpf);
            _ = original.Email.Should().Be(created.Email);
        });
    }

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenClientDoesNotExist()
    {
        var nonExistingId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/clientes?id={nonExistingId}");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Cliente não encontrado.");
        });
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenClientDoesNotExist()
    {
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new
        {
            Nome = "Cliente Atualizado",
            Telefone = "11988888888",
            Email = "cliente.atualizado@email.com"
        };

        var response = await _client.PutAsJsonAsync($"/api/clientes/{nonExistingId}", updateRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Cliente não encontrado.");
        });
    }

    [Test]
    public async Task GetPaged_ShouldReturnFirstPageWithTenItems_WhenTwentyClientsExist()
    {
        for (var i = 1; i <= 20; i++)
            _ = await CreateClientAsync(i);

        var response = await _client.GetAsync("/api/clientes?pageNumber=1&pageSize=10");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        var clientes = root.GetProperty("clientes");

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = clientes.GetArrayLength().Should().Be(10);
        });
    }

    [Test]
    public async Task GetPaged_ShouldReturnSecondPageWithRemainingItems_WhenTwentyClientsExist()
    {
        for (var i = 1; i <= 20; i++)
            _ = await CreateClientAsync(i);

        var response = await _client.GetAsync("/api/clientes?pageNumber=2&pageSize=10");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        var clientes = root.GetProperty("clientes");
        _ = clientes.GetArrayLength().Should().Be(10);

        var firstClientOnPage = clientes[0];
        var nome = firstClientOnPage.GetProperty("nome").GetString();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = clientes.GetArrayLength().Should().Be(10);
            _ = nome.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Test]
    public async Task GetPaged_ShouldReturnEmptyPage_WhenPageNumberExceedsAvailableData()
    {
        for (var i = 1; i <= 20; i++)
            _ = await CreateClientAsync(i);

        var response = await _client.GetAsync("/api/clientes?pageNumber=3&pageSize=10");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        var clientes = root.GetProperty("clientes");

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = clientes.GetArrayLength().Should().Be(0);
        });
    }

    [Test]
    public async Task GetByCpf_ShouldReturnClient_WhenCpfExists()
    {
        var created = await CreateClientAsync(100);

        var response = await _client.GetAsync($"/api/clientes?cpf={created.Cpf}");
        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = cliente.Should().NotBeNull();
            _ = cliente!.Id.Should().Be(created.Id);
            _ = cliente.Cpf.Should().Be(created.Cpf);
        });
    }

    [Test]
    public async Task GetByCpf_ShouldReturnNotFound_WhenCpfDoesNotExist()
    {
        var nonExistingCpf = GenerateValidCpf(9999);

        var response = await _client.GetAsync($"/api/clientes?cpf={nonExistingCpf}");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Cliente não encontrado.");
        });
    }

    [Test]
    public async Task Get_ShouldReturnBadRequest_WhenMoreThanOneFilterIsProvided()
    {
        var created = await CreateClientAsync(200);

        var response = await _client.GetAsync($"/api/clientes?id={created.Id}&cpf={created.Cpf}");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("Informe apenas um filtro por vez: id, cpf ou cnpj.");
        });
    }

    [Test]
    public async Task GetPaged_ShouldReturnBadRequest_WhenPageNumberIsZero()
    {
        var response = await _client.GetAsync("/api/clientes?pageNumber=0&pageSize=10");
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            _ = error.Should().NotBeNull();
            _ = error!.Error.Should().Be("O número da página deve ser maior que zero.");
        });
    }

    private async Task<ClienteResponse> CreateClientAsync(int seed)
    {
        var request = new
        {
            Nome = $"Cliente {seed:D2}",
            Cpf = GenerateValidCpf(seed),
            Telefone = $"1199{seed:D7}",
            Email = $"cliente{seed:D2}@email.com"
        };

        var response = await _client.PostAsJsonAsync("/api/clientes", request);
        var created = await response.Content.ReadFromJsonAsync<ClienteResponse>();

        Assert.Multiple(() =>
        {
            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = created.Should().NotBeNull();
        });

        return created!;
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
        public string Nome { get; set; } = string.Empty;
        public string? Cpf { get; set; }
        public string? Cnpj { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    private sealed class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}