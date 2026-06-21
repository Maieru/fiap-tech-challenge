using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using System.Text.RegularExpressions;

namespace FIAP.TechChallenge.Fase1.Domain.ValueObjects;

public sealed class Cnpj : IEquatable<Cnpj>
{
    public string Value { get; }
    public string Formatted => ConvertToFormatted(Value);
    public string Unformatted => Value;

    private Cnpj(string value)
    {
        Value = value;
    }

    public static Result<Cnpj> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<Cnpj>.Failure(new Error("O CNPJ deve ser informado."));

        var normalized = Normalize(input);

        var validationResult = IsValid(normalized);

        if (!validationResult.IsSuccess)
            return Result<Cnpj>.Failure(validationResult.Error);

        return Result<Cnpj>.Success(new Cnpj(normalized));
    }

    private static string Normalize(string input) => Regex.Replace(input.Trim().ToUpperInvariant(), @"[^A-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromMilliseconds(100));

    private static Result<bool> IsValid(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return Result<bool>.Failure(new Error("O CNPJ deve ser informado."));

        if (cnpj.Length != 14)
            return Result<bool>.Failure(new Error("O CNPJ precisa ter exatamente 14 caracteres."));

        if (!cnpj.All(char.IsLetterOrDigit))
            return Result<bool>.Failure(new Error("O CNPJ deve conter apenas letras e números."));

        var basePart = cnpj[..12];
        var dvPart = cnpj[12..];

        if (!dvPart.All(char.IsDigit))
            return Result<bool>.Failure(new Error("Os dígitos verificadores do CNPJ devem ser numéricos."));

        if (AllCharactersAreEqual(cnpj))
            return Result<bool>.Failure(new Error("O CNPJ informado é inválido."));

        var firstDigit = CalculateVerificationDigit(basePart, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

        if (dvPart[0] != firstDigit)
            return Result<bool>.Failure(new Error("O CNPJ informado é inválido: o primeiro dígito verificador não confere."));

        var secondDigit = CalculateVerificationDigit(basePart + firstDigit, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

        if (dvPart[1] != secondDigit)
            return Result<bool>.Failure(new Error("O CNPJ informado é inválido: o segundo dígito verificador não confere."));

        return Result<bool>.Success(true);
    }

    private static int GetCalculationValue(char character)
    {
        if (char.IsDigit(character))
            return character - '0';

        return character - 48;
    }

    private static char CalculateVerificationDigit(string value, int[] weights)
    {
        var sum = 0;

        for (var i = 0; i < value.Length; i++)
            sum += GetCalculationValue(value[i]) * weights[i];

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return (char)('0' + digit);
    }

    private static bool AllCharactersAreEqual(string value) => value.All(c => c == value[0]);

    private static string ConvertToFormatted(string cnpj) => $"{cnpj[..2]}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";

    public override string ToString() => Formatted;

    public bool Equals(Cnpj? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as Cnpj);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Cnpj? left, Cnpj? right) => Equals(left, right);

    public static bool operator !=(Cnpj? left, Cnpj? right) => !Equals(left, right);
}
