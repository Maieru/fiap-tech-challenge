using Maieru.FiapTechChallenge.Chassis;
using System.Diagnostics;

namespace FIAP.TechChallenge.OS.Application.Messaging;

public sealed class MessageContext
{
    public Guid? IncomingMessageId { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    public MessageEnvelope<T> Create<T>(string type, T payload, DateTimeOffset occurredAt) =>
        new(Guid.NewGuid(), type, OSMessaging.Version, occurredAt, CorrelationId, IncomingMessageId,
            Activity.Current?.Id, Activity.Current?.TraceStateString, payload);
}
