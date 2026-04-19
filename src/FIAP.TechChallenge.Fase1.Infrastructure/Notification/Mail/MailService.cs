using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Notification.Mail;

public sealed class MailService : IMailService
{
    public async Task<Result<bool>> SendMail(string to, string subject, string body)
    {
        Console.WriteLine($"Sending mail to: {to}. Subject {subject}. Body {body}");
        return Result<bool>.Success(true);
    }
}
