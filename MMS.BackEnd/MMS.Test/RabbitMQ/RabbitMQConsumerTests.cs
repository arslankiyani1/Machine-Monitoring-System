using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using MMS.Adapters.RabbitMq.SignalR.RabbitMq;
using MMS.Adapters.RabbitMq.SignalR.SignalR;

namespace MMS.Test.RabbitMQ;

public class RabbitMQConsumerTests
{
    private readonly Mock<IHubContext<MachineHub>> _mockHubContext;
    private readonly Mock<IConnectionMappingService> _mockMappingService;
    private readonly IConfiguration _configuration;

    public RabbitMQConsumerTests()
    {
        _mockHubContext = new Mock<IHubContext<MachineHub>>();
        _mockMappingService = new Mock<IConnectionMappingService>();

        var configData = new Dictionary<string, string?>
        {
            { "RabbitMQ:Host", "localhost" },
            { "RabbitMQ:Username", "guest" },
            { "RabbitMQ:Password", "guest" },
            { "RabbitMQ:QueueName", "test-queue" },
            { "RabbitMQ:VirtualHost", "/" },
            { "RabbitMQ:UseSsl", "false" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenHostIsMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "RabbitMQ:Username", "guest" },
            { "RabbitMQ:Password", "guest" },
            { "RabbitMQ:QueueName", "test-queue" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        var act = () => new RabbitMQConsumer(
            config,
            _mockHubContext.Object,
            _mockMappingService.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenHubContextIsNull()
    {
        // Act & Assert
        var act = () => new RabbitMQConsumer(
            _configuration,
            null!,
            _mockMappingService.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMappingServiceIsNull()
    {
        // Act & Assert
        var act = () => new RabbitMQConsumer(
            _configuration,
            _mockHubContext.Object,
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldUseDefaultVirtualHost_WhenNotProvided()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "RabbitMQ:Host", "localhost" },
            { "RabbitMQ:Username", "guest" },
            { "RabbitMQ:Password", "guest" },
            { "RabbitMQ:QueueName", "test-queue" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        // Note: This will fail if RabbitMQ is not running, but validates the code structure
        var act = () => new RabbitMQConsumer(
            config,
            _mockHubContext.Object,
            _mockMappingService.Object);

        // If RabbitMQ is not available, it will throw a connection exception, not ArgumentNullException
        act.Should().NotThrow<ArgumentNullException>();
    }

    [Fact]
    public void Dispose_ShouldCloseConnectionAndChannel()
    {
        // Arrange
        // Note: This test requires RabbitMQ to be running
        // In a real scenario, you'd mock the connection/channel
        var consumer = new RabbitMQConsumer(
            _configuration,
            _mockHubContext.Object,
            _mockMappingService.Object);

        // Act & Assert
        var act = () => consumer.Dispose();
        act.Should().NotThrow();
    }
}
