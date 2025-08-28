using DotNetEnv.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskManagementSystem.EmailService.Providers;
using TaskManagementSystem.EmailService.Workers;
using TaskManagementSystem.Messaging.Abstractions.Interfaces;
using TaskManagementSystem.Messaging.Email.Interfaces;
using TaskManagementSystem.Messaging.Email.Smtp;
using TaskManagementSystem.Messaging.Email.Smtp.Interfaces;
using TaskManagementSystem.Messaging.RabbitMq;
using TaskManagementSystem.Messaging.Serializers;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddDotNetEnv();
    })
    .ConfigureServices((ctx, services) =>
    {
        // Конфиги из appsettings
        services.Configure<RabbitMqOptions>(ctx.Configuration.GetSection("RabbitMq"));

        // RabbitMQ connection
        services.AddSingleton<IConnection>(sv =>
        {
            var factory = new ConnectionFactory();
            var options = sv.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            factory.HostName = options.Host;
            factory.Port = options.Port;
            factory.VirtualHost = options.VirtualHost;
            factory.UserName = options.UserName;
            factory.Password = options.Password;
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        // Регаем нужные тебе реализации
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<IMessageConsumer>(sv =>
        {
            var conn = sv.GetRequiredService<IConnection>();
            return RabbitConsumer.CreateAsync(
                connection: conn,
                cancellationToken: CancellationToken.None
            ).GetAwaiter().GetResult();
        });

        // IConfiguration
        services.AddSingleton(ctx.Configuration);

        // Email services
        services.AddSingleton<ISmtpConnectionOptionsProvider, SmtpConnectionOptionsProvider>();

        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        // Hosted service, чтобы запустить consumer
        services.AddHostedService<EmailSenderWorker>();
    })
    .Build();

var smtpOops = host.Services.GetService<IOptions<ISmtpConnectionOptionsProvider>>();

await host.RunAsync();
