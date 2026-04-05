namespace FIAP.TechChallenge.Fase1.Domain.Abstractions;

public record Error(string Description)
{
    public static Error None { get; } = new("No error occurred.");

    public static Error InvalidId(string entity) => new Error($"Invalid {entity} id.");

    public static Error NotFound(string entity) => new Error($"{entity} not found.");

    public static Error NoAccess(string entity) => new Error($"You do not have access to this {entity}.");

    public static Error InvalidValue(string entity, string field, string value) => new Error($"Invalid value '{value}' for field '{field}' in {entity}.");
}