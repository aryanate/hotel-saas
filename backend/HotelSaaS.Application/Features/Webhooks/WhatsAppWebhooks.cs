using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelSaaS.Application.Features.Webhooks;

public record HandleWhatsAppWebhookCommand(string Payload) : IRequest<bool>;

public class HandleWhatsAppWebhookCommandHandler : IRequestHandler<HandleWhatsAppWebhookCommand, bool>
{
    private readonly ILogger<HandleWhatsAppWebhookCommandHandler> _logger;

    public HandleWhatsAppWebhookCommandHandler(ILogger<HandleWhatsAppWebhookCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(HandleWhatsAppWebhookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received WhatsApp webhook: {Payload}", request.Payload);
        return Task.FromResult(true);
    }
}

public record VerifyWhatsAppWebhookQuery(
    string Mode,
    string Token,
    string Challenge) : IRequest<string?>;

public class VerifyWhatsAppWebhookQueryHandler : IRequestHandler<VerifyWhatsAppWebhookQuery, string?>
{
    private readonly ILogger<VerifyWhatsAppWebhookQueryHandler> _logger;

    public VerifyWhatsAppWebhookQueryHandler(ILogger<VerifyWhatsAppWebhookQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<string?> Handle(VerifyWhatsAppWebhookQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying WhatsApp webhook challenge: Mode={Mode}, Token={Token}", request.Mode, request.Token);
        // Returns the challenge if valid mode, or challenge string directly
        if (request.Mode == "subscribe")
        {
            return Task.FromResult<string?>(request.Challenge);
        }

        return Task.FromResult<string?>(null);
    }
}
