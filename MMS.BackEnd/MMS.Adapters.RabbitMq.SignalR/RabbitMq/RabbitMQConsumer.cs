namespace MMS.Adapters.RabbitMq.SignalR.RabbitMq;

public class RabbitMQConsumer : BackgroundService
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;
    private readonly string _virtualHost;
    private readonly bool _useSsl;
    private readonly IHubContext<MachineHub> _hubContext;
    private readonly IConnectionMappingService _mappingService;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConsumer(
        IConfiguration configuration,
        IHubContext<MachineHub> hubContext,
        IConnectionMappingService mappingService)
    {
        _hostname = configuration["RabbitMQ:Host"]
            ?? throw new ArgumentNullException(nameof(_hostname));
        _username = configuration["RabbitMQ:Username"]
            ?? throw new ArgumentNullException(nameof(_username));
        _password = configuration["RabbitMQ:Password"]
            ?? throw new ArgumentNullException(nameof(_password));
        _queueName = configuration["RabbitMQ:QueueName"]
            ?? throw new ArgumentNullException(nameof(_queueName));
        _virtualHost = configuration["RabbitMQ:VirtualHost"]
            ?? "/";
        _useSsl = bool.TryParse(configuration["RabbitMQ:UseSsl"], out var ssl) && ssl;

        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));

        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = CreateConnectionFactory();

            _connection = factory.CreateConnection();
            _connection.ConnectionShutdown += OnConnectionShutdown;

            _channel = _connection.CreateModel();
            _channel.ModelShutdown += OnChannelShutdown;

            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            Console.WriteLine($"✅ Connected to RabbitMQ on {_hostname}, queue: {_queueName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to initialize RabbitMQ connection: {ex.Message}");
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

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
    {
        Console.WriteLine($"⚠️ RabbitMQ connection shutdown: {reason.ReplyText}");
    }

    private void OnChannelShutdown(object? sender, ShutdownEventArgs reason)
    {
        Console.WriteLine($"⚠️ RabbitMQ channel shutdown: {reason.ReplyText}");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized.");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);

            try
            {
                var log = JsonConvert.DeserializeObject<MachineLogSignalRDto>(jsonMessage);

                if (log != null)
                {
                    // ✅ Support both customer and machine groups
                    var responseDto = new ApiResponse<MachineLogSignalRDto>(
                        StatusCodes.Status200OK,
                        "New log created.",
                        log
                    );

                    if (log.CustomerId != Guid.Empty)
                    {
                        await _hubContext.Clients.Group(log.CustomerId.ToString())
                            .SendAsync("ReceiveMachineUpdate", responseDto);
                    }

                    if (!string.IsNullOrWhiteSpace(log.Name))
                    {
                        await _hubContext.Clients.Group(log.Name)
                            .SendAsync("ReceiveMachineUpdate", responseDto);
                    }

                    Console.WriteLine($"[SignalR] Sent log update for {log.Name} / {log.CustomerId}");
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    Console.WriteLine("❗ Invalid log payload (null or empty).");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing message: {ex.Message}");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
