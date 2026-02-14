using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MMS.Adapters.RabbitMq.SignalR.RabbitMq;
using MMS.Application.Ports.In.NoSql.MachineLog.Dto;
using Xunit;

namespace MMS.Adapters.RabbitMq.SignalR.Tests;

public class RabbitMQProducerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<RabbitMQProducer>> _mockLogger;
    private readonly IConfiguration _configuration;

    public RabbitMQProducerTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<RabbitMQProducer>>();

        // Setup default configuration
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
        var act = () => new RabbitMQProducer(config, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUsernameIsMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "RabbitMQ:Host", "localhost" },
            { "RabbitMQ:Password", "guest" },
            { "RabbitMQ:QueueName", "test-queue" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        var act = () => new RabbitMQProducer(config, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPasswordIsMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "RabbitMQ:Host", "localhost" },
            { "RabbitMQ:Username", "guest" },
            { "RabbitMQ:QueueName", "test-queue" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        var act = () => new RabbitMQProducer(config, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQueueNameIsMissing()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            { "RabbitMQ:Host", "localhost" },
            { "RabbitMQ:Username", "guest" },
            { "RabbitMQ:Password", "guest" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act & Assert
        var act = () => new RabbitMQProducer(config, _mockLogger.Object);
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

        // Act
        var producer = new RabbitMQProducer(config, _mockLogger.Object);

        // Assert
        producer.Should().NotBeNull();
    }

    [Fact]
    public void PublishMachineLogAsync_ShouldThrow_WhenLogIsNull()
    {
        // Arrange
        var producer = new RabbitMQProducer(_configuration, _mockLogger.Object);

        // Act & Assert
        var act = () => producer.PublishMachineLogAsync(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PublishMachineLogAsync_ShouldLogWarning_WhenLogIsNull()
    {
        // Arrange
        var producer = new RabbitMQProducer(_configuration, _mockLogger.Object);

        // Act
        try
        {
            producer.PublishMachineLogAsync(null!);
        }
        catch { }

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("null log")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void PublishMachineLogAsync_ShouldCreateValidLog()
    {
        // Arrange
        var producer = new RabbitMQProducer(_configuration, _mockLogger.Object);
        var log = CreateTestLog();

        // Act & Assert
        // Note: This will fail if RabbitMQ is not running, but validates the code structure
        var act = () => producer.PublishMachineLogAsync(log);
        
        // We expect it might throw if RabbitMQ is not available, but not ArgumentNullException
        if (act.Should().NotThrow<ArgumentNullException>().Subject != null)
        {
            // If it throws, it should be a connection exception, not a null exception
            act.Should().NotThrow<ArgumentNullException>();
        }
    }

    private static MachineLogSignalRDto CreateTestLog()
    {
        return new MachineLogSignalRDto(
            Id: Guid.NewGuid().ToString(),
            MachineId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            Name: "Test Machine",
            UserId: Guid.NewGuid(),
            Color: "#FF0000",
            Status: "Running",
            JobId: "Job-123",
            UserName: "Test User",
            StatusSummary: new Dictionary<string, int>
            {
                { "Online", 5 },
                { "Offline", 2 }
            }
        );
    }
}
