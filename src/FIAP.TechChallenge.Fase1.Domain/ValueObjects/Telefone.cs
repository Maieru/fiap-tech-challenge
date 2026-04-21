using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using System.Text.RegularExpressions;

namespace FIAP.TechChallenge.Fase1.Domain.ValueObjects;

public sealed class Telefone : IEquatable<Telefone>
{
    public string Value { get; }
    public string Unformatted => Value;
    public string Formatted => ConvertToFormatted(Value);
    public TipoTelefone Tipo => DetermineType(Value);

    public bool IsMobile => Tipo == TipoTelefone.Movel;
    public bool IsLandline => Tipo == TipoTelefone.Fixo;

    private Telefone(string value)
    {
        Value = value;
    }

    public static Result<Telefone> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<Telefone>.Failure(new Error("O telefone deve ser informado."));

        var normalized = Normalize(input);

        var validationResult = IsValid(normalized);

        if (!validationResult.IsSuccess)
            return Result<Telefone>.Failure(validationResult.Error);

        return Result<Telefone>.Success(new Telefone(normalized));
    }

    private static string Normalize(string input) => Regex.Replace(input, @"\D", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));

    private static Result<bool> IsValid(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return Result<bool>.Failure(new Error("O telefone deve ser informado."));

        if (!telefone.All(char.IsDigit))
            return Result<bool>.Failure(new Error("O telefone deve conter apenas números."));

        if (telefone.Length is not 10 and not 11)
            return Result<bool>.Failure(new Error("O telefone deve ter 10 dígitos para fixo ou 11 dígitos para móvel."));

        var ddd = telefone[..2];

        if (!IsValidDdd(ddd))
            return Result<bool>.Failure(new Error("O telefone informado possui um DDD inválido."));

        if (telefone.Length == 11 && telefone[2] != '9')
            return Result<bool>.Failure(new Error("O telefone móvel informado é inválido: ele deve começar com 9 após o DDD."));

        if (telefone.Length == 10 && telefone[2] == '9')
            return Result<bool>.Failure(new Error("O telefone fixo informado é inválido."));

        if (AllDigitsAreEqual(telefone))
            return Result<bool>.Failure(new Error("O telefone informado é inválido."));

        return Result<bool>.Success(true);
    }

    private static bool IsValidDdd(string ddd)
    {
        if (ddd.Length != 2 || !ddd.All(char.IsDigit))
            return false;

        if (ddd[0] == '0')
            return false;

        return true;
    }

    private static bool AllDigitsAreEqual(string value) => value.All(c => c == value[0]);

    private static TipoTelefone DetermineType(string telefone) => telefone.Length == 11 ? TipoTelefone.Movel : TipoTelefone.Fixo;

    private static string ConvertToFormatted(string telefone)
    {
        var ddd = telefone[..2];
        var number = telefone[2..];

        if (telefone.Length == 11)
            return $"({ddd}) {number[..5]}-{number[5..]}";

        return $"({ddd}) {number[..4]}-{number[4..]}";
    }

    public override string ToString() => Formatted;

    public bool Equals(Telefone? other)
    {
        if (other is null)
            return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as Telefone);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Telefone? left, Telefone? right) => Equals(left, right);

    public static bool operator !=(Telefone? left, Telefone? right) => !Equals(left, right);
}