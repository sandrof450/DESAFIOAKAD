using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroServicoVendas.RabbitMQ.Consumers
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly ConnectionFactory _factory;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger)
        {
            Console.WriteLine($"[DEBUG] RabbitMQ URI: {configuration["RabbitMQ:Uri"] ?? "amqps://rsclvuno:***@jaragua.lmq.cloudamqp.com/rsclvuno"}");
            Console.WriteLine($"[DEBUG] RabbitMQ Queue: {configuration["RabbitMQ:Queue"] ?? "vendaNotification"}");
            var uri = new Uri(configuration["RabbitMQ:Uri"] ?? "amqps://rsclvuno:***@jaragua.lmq.cloudamqp.com/rsclvuno");
            _logger = logger;
            _queueName = configuration["RabbitMQ:Queue"] ?? "vendaNotification";
            _factory = new ConnectionFactory
            {
                Uri = uri,
                DispatchConsumersAsync = true //Importante para AsyncEvetingBasicConsumer
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declara fila (importante para garantir que a fila exista)
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Prefetch para controlar quanto cada consumidor processa simultaneamente
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    _logger.LogInformation($"Nova mensagem recebida na fila...\n\n\n\n");

                    var body = ea.Body.ToArray();// converte ReadOnlyMemory<byte> em byte[]
                    var mensagem = Encoding.UTF8.GetString(body);// transforma em string
                    // var evento = JsonSerializer.Deserialize<NotificacaoVendaDTO>(json);

                    // PROCESSAMENTO: Substituir por lógica real
                    _logger.LogInformation($"{mensagem}");
                    // Exemplo: Salvar mensagem em arquivo local
                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mensagens_recebidas.txt");
                    await File.AppendAllTextAsync(filePath, $"{DateTime.UtcNow}: {mensagem}{Environment.NewLine}");

                    // Simula processamento async
                    await Task.Yield();

                    // Ack manual (confirmar processamento)
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem. Enviando para DLQ ou descartando conforme política.");

                    // Nack: requeue = false para enviar à dead-letter(se configurada) ou descartar
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            // Mantém a task até o cancellation
            var tcs = new TaskCompletionSource<object?>();
            stoppingToken.Register(() => tcs.SetResult(null));
            return tcs.Task;
        }

        public override void Dispose()
        {
            try
            { _channel?.Close();} catch{ }
            try { _connection?.Close(); } catch {}
            base.Dispose();
        }             
    }
}