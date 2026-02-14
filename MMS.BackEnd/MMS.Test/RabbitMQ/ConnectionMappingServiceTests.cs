using FluentAssertions;
using MMS.Adapters.RabbitMq.SignalR;

namespace MMS.Test.RabbitMQ;

public class ConnectionMappingServiceTests
{
    private readonly ConnectionMappingService _service;

    public ConnectionMappingServiceTests()
    {
        _service = new ConnectionMappingService();
    }

    [Fact]
    public void Add_ShouldStoreMapping()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var connectionId = "connection-123";

        // Act
        _service.Add(machineId, connectionId);

        // Assert
        var result = _service.Get(machineId);
        result.Should().Be(connectionId);
    }

    [Fact]
    public void Add_ShouldUpdateExistingMapping()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var connectionId1 = "connection-1";
        var connectionId2 = "connection-2";

        // Act
        _service.Add(machineId, connectionId1);
        _service.Add(machineId, connectionId2);

        // Assert
        var result = _service.Get(machineId);
        result.Should().Be(connectionId2);
    }

    [Fact]
    public void Get_ShouldReturnNull_WhenMachineNotMapped()
    {
        // Arrange
        var machineId = Guid.NewGuid();

        // Act
        var result = _service.Get(machineId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldRemoveMapping()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var connectionId = "connection-123";
        _service.Add(machineId, connectionId);

        // Act
        _service.Remove(connectionId);

        // Assert
        var result = _service.Get(machineId);
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldHandleNonExistentConnection()
    {
        // Arrange
        var connectionId = "non-existent-connection";

        // Act & Assert
        var act = () => _service.Remove(connectionId);
        act.Should().NotThrow();
    }

    [Fact]
    public void MultipleMachines_ShouldWorkIndependently()
    {
        // Arrange
        var machineId1 = Guid.NewGuid();
        var machineId2 = Guid.NewGuid();
        var connectionId1 = "connection-1";
        var connectionId2 = "connection-2";

        // Act
        _service.Add(machineId1, connectionId1);
        _service.Add(machineId2, connectionId2);

        // Assert
        _service.Get(machineId1).Should().Be(connectionId1);
        _service.Get(machineId2).Should().Be(connectionId2);
    }

    [Fact]
    public void Remove_ShouldOnlyRemoveSpecifiedConnection()
    {
        // Arrange
        var machineId1 = Guid.NewGuid();
        var machineId2 = Guid.NewGuid();
        var connectionId1 = "connection-1";
        var connectionId2 = "connection-2";

        _service.Add(machineId1, connectionId1);
        _service.Add(machineId2, connectionId2);

        // Act
        _service.Remove(connectionId1);

        // Assert
        _service.Get(machineId1).Should().BeNull();
        _service.Get(machineId2).Should().Be(connectionId2);
    }
}
