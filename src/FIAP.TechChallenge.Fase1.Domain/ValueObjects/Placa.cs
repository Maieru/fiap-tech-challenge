using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using System.Text.RegularExpressions;

namespace FIAP.TechChallenge.Fase1.Domain.ValueObjects;

public sealed class Placa : IEquatable<Placa>
{
    private static readonly Regex PadraoAntigoRegex = new(@"^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex MercosulRegex = new(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public string Value { get; }
    public string Unformatted => Value;
    public bool IsPadraoAntigo => PadraoAntigoRegex.IsMatch(Value);
    public bool IsMercosul => MercosulRegex.IsMatch(Value);

    private Placa(string value)
    {
        Value = value;
    }

    public static Result<Placa> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<Placa>.Failure(new Error("A placa deve ser informada."));

        var normalized = Normalize(input);

        var validationResult = IsValid(normalized);

        if (!validationResult.IsSuccess)
            return Result<Placa>.Failure(validationResult.Error);

        return Result<Placa>.Success(new Placa(normalized));
    }

    private static Result<bool> IsValid(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<bool>.Failure(new Error("A placa deve ser informada."));

        var placa = Normalize(input);

        if (string.IsNullOrWhiteSpace(placa))
            return Result<bool>.Failure(new Error("A placa deve ser informada."));

        if (placa.Length != 7)
            return Result<bool>.Failure(new Error("A placa deve ter exatamente 7 caracteres alfanuméricos."));

        if (!placa.All(char.IsLetterOrDigit))
            return Result<bool>.Failure(new Error("A placa deve conter apenas letras e números."));

        if (!PadraoAntigoRegex.IsMatch(placa) && !MercosulRegex.IsMatch(placa))
            return Result<bool>.Failure(new Error("A placa informada é inválida. Use o padrão antigo (AAA1234) ou Mercosul (AAA1A23)."));

        return Result<bool>.Success(true);
    }

    private static string Normalize(string input) => Regex.Replace(input, @"[^a-zA-Z0-9]", "", RegexOptions.None, TimeSpan.FromMilliseconds(100)).ToUpperInvariant();

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as Placa);

    public bool Equals(Placa? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Placa? left, Placa? right) => Equals(left, right);

    public static bool operator !=(Placa? left, Placa? right) => !Equals(left, right);
}

