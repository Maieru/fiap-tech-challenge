using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Login { get; private set; }
    public string Senha { get; private set; }

    private Usuario(Guid id, string login, string senhaCriptografada)
    {
        Id = id;
        Login = login;
        Senha = senhaCriptografada;
    }

    public static Result<Usuario> Create(string login, string senhaCriptografada)
    {
        return Create(Guid.NewGuid(), login, senhaCriptografada);
    }

    public static Result<Usuario> Rehydrate(Guid id, string login, string senhaCriptografada)
    {
        if (id == Guid.Empty)
            return Result<Usuario>.Failure(new Error("O id do usuário é inválido."));

        return Create(id, login, senhaCriptografada);
    }

    private static Result<Usuario> Create(Guid id, string login, string senhaCriptografada)
    {
        var loginResult = ValidateLogin(login);
        if (!loginResult.IsSuccess || loginResult.Value is null)
            return Result<Usuario>.Failure(loginResult.Error);

        var senhaCriptografadaResult = ValidateSenhaCriptografada(senhaCriptografada);
        if (!senhaCriptografadaResult.IsSuccess || senhaCriptografadaResult.Value is null)
            return Result<Usuario>.Failure(senhaCriptografadaResult.Error);

        return Result<Usuario>.Success(new Usuario(id, loginResult.Value, senhaCriptografadaResult.Value));
    }

    private static Result<string> ValidateLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return Result<string>.Failure(new Error("O usuário é obrigatório."));

        var normalized = login.Trim().ToLowerInvariant();

        if (normalized.Length < 3)
            return Result<string>.Failure(new Error("O usuário deve ter no mínimo 3 caracteres."));

        if (normalized.Length > 100)
            return Result<string>.Failure(new Error("O usuário deve ter no máximo 100 caracteres."));

        return Result<string>.Success(normalized);
    }

    private static Result<string> ValidateSenhaCriptografada(string senhaCriptografada)
    {
        if (string.IsNullOrWhiteSpace(senhaCriptografada))
            return Result<string>.Failure(new Error("A senha criptografada é obrigatória."));

        return Result<string>.Success(senhaCriptografada.Trim());
    }
}
