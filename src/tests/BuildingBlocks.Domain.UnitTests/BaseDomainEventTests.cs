using Xunit;

namespace BuildingBlocks.Domain.UnitTests;

public class BaseDomainEventTests
{
    private class TestDomainEvent : BaseDomainEvent { }

    [Fact]
    public void Constructor_GeneratesId()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent();

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.Id);
    }

    [Fact]
    public void Constructor_SetsOccurredOn()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent();

        // Assert
        Assert.True(domainEvent.OccurredOn <= DateTimeOffset.UtcNow);
        Assert.True(domainEvent.OccurredOn > DateTimeOffset.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        // Arrange & Act
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Assert
        Assert.NotEqual(event1.Id, event2.Id);
    }
} 