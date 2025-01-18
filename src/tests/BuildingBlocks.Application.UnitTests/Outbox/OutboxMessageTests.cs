using Xunit;
using BuildingBlocks.Application.Outbox;

namespace BuildingBlocks.Application.UnitTests.Outbox;

public class OutboxMessageTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var type = "TestType";
        var data = "TestData";

        // Act
        var message = new OutboxMessage(type, data);

        // Assert
        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(type, message.Type);
        Assert.Equal(data, message.Data);
        Assert.True(message.OccurredOn <= DateTime.UtcNow);
        Assert.True(message.OccurredOn > DateTime.UtcNow.AddSeconds(-1));
        Assert.Null(message.ProcessedDate);
    }

    [Fact]
    public void MarkAsProcessed_SetsProcessedDate()
    {
        // Arrange
        var message = new OutboxMessage("TestType", "TestData");

        // Act
        message.MarkAsProcessed();

        // Assert
        Assert.NotNull(message.ProcessedDate);
        Assert.True(message.ProcessedDate <= DateTime.UtcNow);
        Assert.True(message.ProcessedDate > DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        // Arrange & Act
        var message1 = new OutboxMessage("TestType", "TestData");
        var message2 = new OutboxMessage("TestType", "TestData");

        // Assert
        Assert.NotEqual(message1.Id, message2.Id);
    }
} 