# RabbitMQ and SignalR Adapter Tests

This test project contains comprehensive unit tests for the RabbitMQ and SignalR adapter components.

## Test Coverage

### 1. ConnectionMappingServiceTests
- Tests for adding, removing, and retrieving connection mappings
- Tests for handling multiple machines and connections
- Edge case handling (null values, non-existent connections)

### 2. RabbitMQProducerTests
- Configuration validation tests
- Null parameter handling
- Error logging verification
- Connection factory creation

### 3. RabbitMQConsumerTests
- Configuration validation
- Null dependency handling
- Connection initialization
- Disposal handling

### 4. MachineHubTests
- Connection lifecycle (OnConnectedAsync, OnDisconnectedAsync)
- Group management
- Broadcast methods
- Error handling for invalid inputs

## Running Tests

```bash
dotnet test
```

## Issues Fixed

### RabbitMQProducer
- ✅ Added error handling and logging
- ✅ Added null parameter validation
- ✅ Made messages persistent
- ✅ Added automatic recovery configuration
- ✅ Improved connection factory setup

### RabbitMQConsumer
- ✅ Added connection shutdown handlers
- ✅ Added automatic recovery configuration
- ✅ Improved error handling
- ✅ Better connection lifecycle management

### MachineHub
- ✅ Fixed inconsistent group management in OnDisconnectedAsync
- ✅ Now uses user_id to get customer groups (consistent with OnConnectedAsync)
- ✅ Improved error handling and logging

## Test Notes

Some tests require RabbitMQ to be running locally. For integration tests, ensure:
- RabbitMQ is installed and running
- Default credentials (guest/guest) are available, or update test configuration
- Test queue can be created

For unit tests that don't require actual RabbitMQ connection, mocks are used.
