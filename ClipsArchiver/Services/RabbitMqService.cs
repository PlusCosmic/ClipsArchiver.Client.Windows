using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace ClipsArchiver.Services;

public static class RabbitMqService
{
    private static IModel? _publishChannel;
    private static IModel? _consumeChannel;
    private static IConnection? _connection;
    private static string? _queueName;
    public static IEventAggregator EventAggregator { get; set; } = new EventAggregator();
    
    private static void Init()
    {
        var factory = new ConnectionFactory
        {
            HostName = "10.0.0.10",
            UserName = "clipsArchiver",
            Password = "clipsArchiver",
            VirtualHost = "/",
            Port= 5672
        };
        _connection = factory.CreateConnection();
        _publishChannel = _connection.CreateModel();
        _consumeChannel = _connection.CreateModel();
        _publishChannel.ExchangeDeclare("clipsArchiverExchange", ExchangeType.Fanout);
        _consumeChannel.ExchangeDeclare("clipsArchiverExchange", ExchangeType.Fanout);
        _queueName = _consumeChannel.QueueDeclare().QueueName;
        _consumeChannel.QueueBind(queue: _queueName,
            exchange: "clipsArchiverExchange",
            routingKey: String.Empty);
        
        var consumer = new EventingBasicConsumer(_consumeChannel);
        consumer.Received += Consume;
        _consumeChannel.BasicConsume(queue: _queueName,
            autoAck: true,
            consumer: consumer);
    }
    
    public static void Publish<T>(T payload)
    {
        if (_publishChannel == null)
        {
            Init();
        }

        var message = JsonConvert.SerializeObject(payload);
        BasicProperties properties = new();
        properties.Headers = new Dictionary<string, object>
        {
            {"type", payload.GetType().Name}
        };
        
        var body = Encoding.UTF8.GetBytes(message);

        _publishChannel.BasicPublish(exchange: "clipsArchiverExchange",
            routingKey: string.Empty,
            basicProperties: properties,
            body: body);
    }

    private static void Consume(object? model, BasicDeliverEventArgs args)
    {
    }
}