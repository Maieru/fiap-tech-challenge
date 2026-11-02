namespace FIAP.TechChallenge.OS.Domain.Abstractions;

public sealed class ConcurrencyException() : Exception("A ordem foi alterada por outra operação.");
