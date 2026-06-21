using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using System.Net.Mail;

namespace FIAP.TechChallenge.Fase1.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email>.Failure(new Error("O e-mail é obrigatório."));

        value = value.Trim();

        var validationResult = IsValid(value);

        if (!validationResult.IsSuccess)
            return Result<Email>.Failure(validationResult.Error);

        return Result<Email>.Success(new Email(value));
    }

    public static Result<bool> IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<bool>.Failure(new Error("O e-mail é obrigatório."));

        value = value.Trim();

        if (value.Length > 200)
            return Result<bool>.Failure(new Error("O e-mail deve ter no máximo 200 caracteres."));

        try
        {
            var address = new MailAddress(value);

            if (address.Address != value)
                return Result<bool>.Failure(new Error("O e-mail informado é inválido."));

            return Result<bool>.Success(true);
        }
        catch
        {
            return Result<bool>.Failure(new Error("O e-mail informado é inválido."));
        }
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as Email);

    public bool Equals(Email? other)
    {
        if (other is null)
            return false;

        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public static bool operator ==(Email? left, Email? right) => Equals(left, right);

    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);
}
