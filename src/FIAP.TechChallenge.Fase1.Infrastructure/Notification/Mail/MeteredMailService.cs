using FIAP.TechChallenge.Fase1.Domain.Observability;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Notification.Mail;

public sealed class MeteredMailService(IMailService inner) : IMailService
{
    public async Task<Result<bool>> SendMail(string to, string subject, string body)
    {
        Result<bool> result;
        try
        {
            result = await inner.SendMail(to, subject, body);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            MetricasNegocio.RegistrarFalhaIntegracao("email");
            throw;
        }

        if (!result.IsSuccess || !result.Value)
            MetricasNegocio.RegistrarFalhaIntegracao("email");
        return result;
    }
}
