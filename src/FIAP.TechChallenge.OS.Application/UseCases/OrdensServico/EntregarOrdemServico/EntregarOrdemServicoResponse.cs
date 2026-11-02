using FIAP.TechChallenge.OS.Domain.Enums;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EntregarOrdemServico;
public sealed record EntregarOrdemServicoResponse(Guid Id, StatusOrdemServico Status, DateTime DataEntrega);
