using MicroServicoEstoque.Infrastructure.RabbitMQ.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace MicroServicoEstoque.Infrastructure.RabbitMQ.Publishers
{
  public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            // Pega do appsettings.json
            var uri = configuration.GetConnectionString("RabbitMq");
            _factory = new ConnectionFactory
            {
                Uri = new Uri(uri)
            };
        }

        public void Publish(string queueName, string message)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Garante que a fila existe
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body);
        }
    }
}