using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.Messaging.Abstractions.DTO;
using TaskManagementSystem.Messaging.Abstractions.Interfaces;
using TaskManagementSystem.Messaging.Email.DTO;
using TaskManagementSystem.Messaging.Email.Interfaces;
using TaskManagementSystem.Messaging.RabbitMq.Constants;
using TaskManagementSystem.Messaging.Types.Enums;

namespace TaskManagementSystem.EmailService.Workers;


public sealed class EmailSenderWorker : BackgroundService
{
    private readonly ILogger<EmailSenderWorker> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IMessageSerializer _serializer;
    private readonly IMessageConsumer _consumer;

    public EmailSenderWorker(
        ILogger<EmailSenderWorker> logger,
        IEmailSender emailSender,
        IMessageSerializer serializer,
        IMessageConsumer consumer)
    {
        _logger = logger;
        _emailSender = emailSender;
        _consumer = consumer;
        _serializer = serializer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting EmailSenderWorker...");

        await _consumer.StartAsync(
            config: ConsumerConfigs.EmailConsumerConfig,
            onMessage: OnMessageReceivedAsync,
            cancellationToken: stoppingToken);
    }

    private async Task<ConsumeResult> OnMessageReceivedAsync(Envelope envelope)
    {
        EmailMessage decodedMessage;
        try
        {
            decodedMessage = _serializer.Deserialize<EmailMessage>(envelope.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to deserialize message: {Error}", ex.Message);
            return ConsumeResult.DeadLetter;
        }
        await _emailSender.SendEmailAsync(decodedMessage);

        _logger.LogInformation("Email send successfully.");

        return ConsumeResult.Ack;
    }
}
