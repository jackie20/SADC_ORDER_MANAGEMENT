using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SADC_Order_Management_System.Configurations;
using System.Text;

namespace SADC_Order_Management_System.Infrastructure.Messaging
{
    public class RabbitMqPublisher
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void Publish(string payload, string type, string messageId, string? correlationId)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
            channel.ConfirmSelect();

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Type = type;
            properties.MessageId = messageId;
            properties.CorrelationId = correlationId;

            channel.BasicPublish(
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(payload));

            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
            _logger.LogInformation("RabbitMQ published message successfully. MessageId={MessageId}", messageId);
        }
    }
}