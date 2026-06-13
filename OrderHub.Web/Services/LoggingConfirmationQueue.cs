using OrderHub.Ordering.Confirmations;

namespace OrderHub.Web.Services;

public sealed class LoggingConfirmationQueue : IConfirmationQueue
{
    private readonly ILogger<LoggingConfirmationQueue> _logger;

    public LoggingConfirmationQueue(ILogger<LoggingConfirmationQueue> logger) => _logger = logger;

    public Task EnqueueAsync(OrderConfirmation confirmation, CancellationToken ct)
    {
        _logger.LogInformation(
            "Queued confirmation for {Email}, subtotal {Subtotal}, intent {IntentId}",
            confirmation.ParentEmail, confirmation.Subtotal, confirmation.PaymentIntentId);

        return Task.CompletedTask;
    }
}