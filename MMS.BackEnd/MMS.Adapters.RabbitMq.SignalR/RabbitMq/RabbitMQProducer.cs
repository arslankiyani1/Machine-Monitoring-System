namespace MMS.Adapters.RabbitMq.SignalR.RabbitMq;

public class RabbitMQProducer : IRabbitMQProducer
{
    private readonly string _hostname;
    private readonly string _username;
    private readonly string _password;
    private readonly string _queueName;
    private readonly string _virtualHost;
    private readonly bool _useSsl;
    private readonly ILogger<RabbitMQProducer>? _logger;

    public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer>? logger = null)
    {
        _hostname = configuration["RabbitMQ:Host"] 
            ?? throw new ArgumentNullException("RabbitMQ:Host");
        _username = configuration["RabbitMQ:Username"] 
            ?? throw new ArgumentNullException("RabbitMQ:Username");
        _password = configuration["RabbitMQ:Password"] 
            ?? throw new ArgumentNullException("RabbitMQ:Password");
        _queueName = configuration["RabbitMQ:QueueName"] 
            ?? throw new ArgumentNullException("RabbitMQ:QueueName");
        _virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
        _useSsl = bool.TryParse(configuration["RabbitMQ:UseSsl"], out var ssl) && ssl;
        _logger = logger;
    }

    public void PublishMachineLogAsync(MachineLogSignalRDto log)
    {
        if (log == null)
        {
            _logger?.LogWarning("Attempted to publish null log");
            throw new ArgumentNullException(nameof(log));
        }

        try
        {
            var factory = CreateConnectionFactory();

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var message = JsonConvert.SerializeObject(log);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // Make message persistent

            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);

            _logger?.LogInformation("[RabbitMQ] Log sent to queue '{QueueName}': {MachineName}", _queueName, log.Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to publish log to RabbitMQ for machine: {MachineName}", log.Name);
            throw;
        }
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            UserName = _username,
            Password = _password,
            VirtualHost = _virtualHost,
            Port = _useSsl ? 5671 : 5672,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        if (_useSsl)
        {
            factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = _hostname,
                Version = SslProtocols.Tls12
            };
        }

        return factory;
    }
}
