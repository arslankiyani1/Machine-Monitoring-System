using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MMS.Adapters.RabbitMq.SignalR.SignalR;
using MMS.Application.Common;
using MMS.Application.Ports.In.User;
using System.Reflection;

namespace MMS.Test.SignalR;

public class MachineHubTests
{
    private readonly Mock<IConnectionMappingService> _mockMappingService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<MachineHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IClientProxy> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockSingleClient;
    private readonly Mock<IHubClients> _mockHubClients;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockRequest;

    public MachineHubTests()
    {
        _mockMappingService = new Mock<IConnectionMappingService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<MachineHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockClients = new Mock<IClientProxy>();
        _mockSingleClient = new Mock<ISingleClientProxy>();
        _mockHubClients = new Mock<IHubClients>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockRequest = new Mock<HttpRequest>();

        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockContext.Setup(c => c.GetHttpContext()).Returns(_mockHttpContext.Object);
        _mockHttpContext.Setup(h => h.Request).Returns(_mockRequest.Object);
        _mockHubClients.Setup(c => c.All).Returns(_mockClients.Object);
        _mockHubClients.Setup(c => c.Client(It.IsAny<string>())).Returns(_mockSingleClient.Object);
        _mockHubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClients.Object);
    }

    private MachineHub CreateHubWithContext()
    {
        var hub = new MachineHub(_mockMappingService.Object, _mockUserService.Object, _mockLogger.Object);
        
        // Use reflection to set Context, Groups, and Clients
        var contextProperty = typeof(Hub).GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance);
        var groupsProperty = typeof(Hub).GetProperty("Groups", BindingFlags.NonPublic | BindingFlags.Instance);
        var clientsProperty = typeof(Hub).GetProperty("Clients", BindingFlags.NonPublic | BindingFlags.Instance);

        contextProperty?.SetValue(hub, _mockContext.Object);
        groupsProperty?.SetValue(hub, _mockGroups.Object);
        clientsProperty?.SetValue(hub, _mockHubClients.Object);

        return hub;
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldThrow_WhenHttpContextIsNull()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.GetHttpContext()).Returns((HttpContext?)null);
        
        var contextProperty = typeof(Hub).GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance);
        contextProperty?.SetValue(hub, mockContext.Object);

        // Act & Assert
        var act = async () => await hub.OnConnectedAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldThrow_WhenUserIdIsMissing()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var queryCollection = new Mock<IQueryCollection>();
        StringValues emptyValues = default;
        queryCollection.Setup(q => q.TryGetValue("user_id", out emptyValues)).Returns(false);
        
        _mockRequest.Setup(r => r.Query).Returns(queryCollection.Object);

        // Act & Assert
        var act = async () => await hub.OnConnectedAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldThrow_WhenUserIdIsInvalid()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var queryCollection = new Mock<IQueryCollection>();
        StringValues invalidUserId = "invalid-guid";
        queryCollection.Setup(q => q.TryGetValue("user_id", out invalidUserId)).Returns(true);
        
        _mockRequest.Setup(r => r.Query).Returns(queryCollection.Object);

        // Act & Assert
        var act = async () => await hub.OnConnectedAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldJoinCustomerGroups_WhenValid()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var userId = Guid.NewGuid();
        var customerIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        
        var queryCollection = new Mock<IQueryCollection>();
        StringValues userIdStr = userId.ToString();
        queryCollection.Setup(q => q.TryGetValue("user_id", out userIdStr)).Returns(true);
        _mockRequest.Setup(r => r.Query).Returns(queryCollection.Object);

        _mockUserService
            .Setup(s => s.GetAccessibleCustomerIdsAsync(userId))
            .ReturnsAsync(new ApiResponse<IEnumerable<string>>(StatusCodes.Status200OK, "Success", customerIds));

        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.OnConnectedAsync();

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync("test-connection-id", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(customerIds.Count));
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldRemoveConnection()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var connectionId = "test-connection-id";

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
        _mockMappingService.Verify(m => m.Remove(connectionId), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldRemoveCustomerGroups()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var userId = Guid.NewGuid();
        var customerIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        
        var queryCollection = new Mock<IQueryCollection>();
        StringValues userIdStr = userId.ToString();
        queryCollection.Setup(q => q.TryGetValue("user_id", out userIdStr)).Returns(true);
        _mockRequest.Setup(r => r.Query).Returns(queryCollection.Object);

        _mockUserService
            .Setup(s => s.GetAccessibleCustomerIdsAsync(userId))
            .ReturnsAsync(new ApiResponse<IEnumerable<string>>(StatusCodes.Status200OK, "Success", customerIds));

        _mockGroups
            .Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync("test-connection-id", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(customerIds.Count));
    }

    [Fact]
    public async Task SendOrderUpdate_ShouldSendToAllClients()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var log = new MachineLog
        {
            Id = Guid.NewGuid().ToString(),
            MachineId = Guid.NewGuid(),
            Status = "Running"
        };

        _mockClients
            .Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.SendOrderUpdate(log);

        // Assert
        _mockClients.Verify(
            c => c.SendAsync("ReceiveMachineUpdate", log, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendOrderUpdateToClient_ShouldSendToSpecificClient()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var connectionId = "test-connection-id";
        var log = new MachineLog
        {
            Id = Guid.NewGuid().ToString(),
            MachineId = Guid.NewGuid(),
            Status = "Running"
        };

        _mockSingleClient
            .Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.SendOrderUpdateToClient(connectionId, log);

        // Assert
        _mockHubClients.Verify(c => c.Client(connectionId), Times.Once);
        _mockSingleClient.Verify(
            c => c.SendAsync("ReceiveMachineUpdate", log, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendOrderUpdateToGroup_ShouldSendToCustomerGroup()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var customerId = Guid.NewGuid();
        var log = new MachineLog
        {
            Id = Guid.NewGuid().ToString(),
            MachineId = Guid.NewGuid(),
            Status = "Running"
        };

        _mockClients
            .Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.SendOrderUpdateToGroup(customerId, log);

        // Assert
        _mockHubClients.Verify(c => c.Group(customerId.ToString()), Times.Once);
        _mockClients.Verify(
            c => c.SendAsync("ReceiveMachineUpdate", log, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendOrderUpdateToMachine_ShouldSendToMachineGroup()
    {
        // Arrange
        var hub = CreateHubWithContext();
        var machineName = "TestMachine";
        var log = new MachineLog
        {
            Id = Guid.NewGuid().ToString(),
            MachineId = Guid.NewGuid(),
            Status = "Running"
        };

        _mockClients
            .Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.SendOrderUpdateToMachine(machineName, log);

        // Assert
        _mockHubClients.Verify(c => c.Group(machineName), Times.Once);
        _mockClients.Verify(
            c => c.SendAsync("ReceiveMachineUpdate", log, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
