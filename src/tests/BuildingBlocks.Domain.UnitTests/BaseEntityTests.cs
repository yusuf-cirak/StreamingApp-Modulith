using BuildingBlocks.Domain;
using Xunit;

namespace BuildingBlocks.Domain.UnitTests;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity<int>
    {
        public TestEntity(int id) : base(id) { }

        public void AddTestEvent(IDomainEvent @event)
        {
            AddDomainEvent(@event);
        }
    }

    private class TestDomainEvent : BaseDomainEvent { }

    [Fact]
    public void Constructor_SetsId()
    {
        // Arrange & Act
        var entity = new TestEntity(1);

        // Assert
        Assert.Equal(1, entity.Id);
    }

    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        // Arrange
        var entity = new TestEntity(1);
        var domainEvent = new TestDomainEvent();

        // Act
        entity.AddTestEvent(domainEvent);

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.Contains(domainEvent, entity.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var entity = new TestEntity(1);
        entity.AddTestEvent(new TestDomainEvent());
        entity.AddTestEvent(new TestDomainEvent());

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameId()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act & Assert
        Assert.True(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentId()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act & Assert
        Assert.False(entity1.Equals(entity2));
    }

    [Fact]
    public void GetHashCode_ReturnsSameValueForSameId()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act & Assert
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }
} 