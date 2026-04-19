using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IMailService
{
    public Task<Result<bool>> SendMail(string to, string subject, string body);
}
