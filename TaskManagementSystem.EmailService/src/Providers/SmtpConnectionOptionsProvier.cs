using Microsoft.Extensions.Configuration;
using TaskManagementSystem.Messaging.Email.Smtp;
using TaskManagementSystem.Messaging.Email.Smtp.Interfaces;
using TaskManagementSystem.Messaging.Types;

namespace TaskManagementSystem.EmailService.Providers;


public sealed class SmtpConnectionOptionsProvider : ISmtpConnectionOptionsProvider
{
    private readonly IConfiguration _configuration;

    public SmtpConnectionOptionsProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public SmtpConnectionOptions GetConnectionOptions()
    {
        return _configuration.GetSection("Smtp").Get<SmtpConnectionOptions>()
               ?? throw new InvalidOperationException("Smtp configuration section is missing or invalid.");
    }
}
