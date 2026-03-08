namespace SADC_Order_Management_System.Configurations
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string Exchange { get; set; } = "orders.exchange";
        public string Queue { get; set; } = "orders.created.queue";
        public string RoutingKey { get; set; } = "orders.created";
        public string DeadLetterExchange { get; set; } = "orders.dlx";
        public string DeadLetterQueue { get; set; } = "orders.dlq";
    }
}