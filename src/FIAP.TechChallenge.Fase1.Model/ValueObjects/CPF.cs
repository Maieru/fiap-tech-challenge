using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using System.Text.RegularExpressions;

namespace FIAP.TechChallenge.Fase1.Domain.ValueObjects;

public sealed class Cpf : IEquatable<Cpf>
{
    public string Value { get; }
    public string Formatted => ConvertToFormatted(Value);
    public string Unformatted => Value;

    private Cpf(string value)
    {
        Value = value;
    }

    public static Result<Cpf> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result<Cpf>.Failure(new Error("O CPF deve ser informado."));

        var digitsOnly = ExtractDigits(input);

        var validationResult = IsValid(digitsOnly);

        if (!validationResult.IsSuccess)
            return Result<Cpf>.Failure(validationResult.Error);

        return Result<Cpf>.Success(new Cpf(digitsOnly));
    }

    private static string ExtractDigits(string input) => Regex.Replace(input, @"\D", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));

    private static Result<bool> IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return Result<bool>.Failure(new Error("O CPF deve ser informado."));

        if (cpf.Length != 11)
            return Result<bool>.Failure(new Error("O CPF precisa ter exatamente 11 dígitos."));

        if (!cpf.All(char.IsDigit))
            return Result<bool>.Failure(new Error("O CPF deve conter apenas números."));

        if (AllDigitsAreEqual(cpf))
            return Result<bool>.Failure(new Error("O CPF informado é inválido."));

        var firstDigit = CalculateVerificationDigit(cpf[..9], 10);
        var secondDigit = CalculateVerificationDigit(cpf[..10], 11);

        if (cpf[9] != firstDigit)
            return Result<bool>.Failure(new Error("O CPF informado é inválido: o primeiro dígito verificador não confere."));

        if (cpf[10] != secondDigit)
            return Result<bool>.Failure(new Error("O CPF informado é inválido: o segundo dígito verificador não confere."));

        return Result<bool>.Success(true);
    }

    private static bool AllDigitsAreEqual(string value) => value.All(c => c == value[0]);

    private static char CalculateVerificationDigit(string source, int weightStart)
    {
        var sum = 0;
        var weight = weightStart;

        foreach (var digitChar in source)
        {
            sum += (digitChar - '0') * weight;
            weight--;
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return (char)('0' + digit);
    }

    private static string ConvertToFormatted(string cpf) => Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");

    public override string ToString() => Formatted;

    public override bool Equals(object? obj) => Equals(obj as Cpf);

    public bool Equals(Cpf? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Cpf? left, Cpf? right) => Equals(left, right);

    public static bool operator !=(Cpf? left, Cpf? right) => !Equals(left, right);
}