using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SADC_Order_Management_System.Configurations;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using System.Text;
using System.Text.Json;

namespace SADC_Order_Management_System.Infrastructure.Messaging
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConsumerService> _logger;

        public RabbitMqConsumerService(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqConsumerService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
            channel.ExchangeDeclare(_options.DeadLetterExchange, ExchangeType.Fanout, durable: true);
            channel.QueueDeclare(_options.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_options.DeadLetterQueue, _options.DeadLetterExchange, string.Empty);

            channel.QueueDeclare(
                _options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = _options.DeadLetterExchange
                });

            channel.QueueBind(_options.Queue, _options.Exchange, _options.RoutingKey);
            channel.BasicQos(0, 10, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, args) =>
            {
                var messageId = args.BasicProperties.MessageId ?? Guid.NewGuid().ToString("N");

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var alreadyProcessed = await db.ProcessedMessages.AnyAsync(x => x.MessageKey == messageId, stoppingToken);
                    if (alreadyProcessed)
                    {
                        channel.BasicAck(args.DeliveryTag, false);
                        return;
                    }

                    var payload = Encoding.UTF8.GetString(args.Body.ToArray());
                    var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(payload)
                                      ?? throw new InvalidOperationException("Invalid message payload.");

                    var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == orderCreated.OrderId, stoppingToken);
                    if (order != null && order.OrderStatus == OrderStatus.Pending)
                    {
                        order.OrderStatus = OrderStatus.Fulfilled;
                        order.UpdatedAt = DateTime.UtcNow;
                    }

                    db.ProcessedMessages.Add(new ProcessedMessage
                    {
                        MessageKey = messageId,
                        ProcessedAtUtc = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync(stoppingToken);
                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ consumer failed for MessageId={MessageId}", messageId);
                    channel.BasicNack(args.DeliveryTag, false, false);
                }
            };

            channel.BasicConsume(_options.Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
    }
}