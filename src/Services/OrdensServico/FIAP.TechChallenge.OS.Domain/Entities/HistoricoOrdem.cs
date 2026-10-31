using System.Text.Json;
using System.Text.Json.Serialization;

namespace FIAP.TechChallenge.OS.Domain.Entities;

public sealed record HistoricoOrdem(Guid Id, Guid OrdemServicoId, string Tipo, DateTime OcorridoEmUtc, string? CorrelationId,
    [property: JsonIgnore] string? DetalhesJson = null)
{
    public JsonElement? Detalhes => DetalhesJson is null ? null : JsonDocument.Parse(DetalhesJson).RootElement.Clone();
}
