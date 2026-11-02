using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Tests.Entities;

public sealed class OrdemServicoTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("ab")]
    public void Create_ShouldRejectInvalidDescription(string? description)
    {
        var result = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), description!);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
        });
    }

    [Test]
    public void Create_ShouldRejectDescriptionOverLimit()
    {
        Assert.That(OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), new string('a', 1001)).IsSuccess, Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Create_ShouldRejectMissingOwner(bool missingCustomer)
    {
        var result = OrdemServico.Create(missingCustomer ? Guid.Empty : Guid.NewGuid(), missingCustomer ? Guid.NewGuid() : Guid.Empty, "Revisao geral");
        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Rehydrate_ShouldRejectMissingIdentityOrDate()
    {
        Assert.Multiple(() =>
        {
            Assert.That(OrdemServico.Rehydrate(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), "Revisao geral", DateTime.UtcNow).IsSuccess, Is.False);
            Assert.That(OrdemServico.Rehydrate(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Revisao geral", default).IsSuccess, Is.False);
        });
    }

    [Test]
    public void Create_ShouldStartReceivedWithNewIdentityAndNormalizedDescription()
    {
        var customer = Guid.NewGuid();
        var vehicle = Guid.NewGuid();
        var result = OrdemServico.Create(customer, vehicle, "  Revisao geral  ");
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.Not.Empty);
            Assert.That(result.Value.ClienteId, Is.EqualTo(customer));
            Assert.That(result.Value.VeiculoId, Is.EqualTo(vehicle));
            Assert.That(result.Value.DescricaoProblema, Is.EqualTo("Revisao geral"));
            Assert.That(result.Value.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(result.Value.DataCriacao.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }
}
