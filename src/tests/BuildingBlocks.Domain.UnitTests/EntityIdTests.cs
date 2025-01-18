using Xunit;

namespace BuildingBlocks.Domain.UnitTests;

public class EntityIdTests
{
    private record TestEntityId(int Value) : EntityId<int>(Value);

    [Fact]
    public void Constructor_SetsValue()
    {
        // Arrange & Act
        var entityId = new TestEntityId(42);

        // Assert
        Assert.Equal(42, entityId.Value);
    }

    [Fact]
    public void GetEqualityComponents_ReturnsValue()
    {
        // Arrange
        var entityId = new TestEntityId(42);

        // Act
        var components = entityId.GetEqualityComponents().ToList();

        // Assert
        Assert.Single(components);
        Assert.Equal(42, components[0]);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameValue()
    {
        // Arrange
        var id1 = new TestEntityId(42);
        var id2 = new TestEntityId(42);

        // Act & Assert
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValue()
    {
        // Arrange
        var id1 = new TestEntityId(42);
        var id2 = new TestEntityId(43);

        // Act & Assert
        Assert.NotEqual(id1, id2);
    }
}

public class GuidEntityIdTests
{
    [Fact]
    public void Constructor_SetsValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var entityId = new GuidEntityId(guid);

        // Assert
        Assert.Equal(guid, entityId.Value);
    }

    [Fact]
    public void Create_GeneratesNewGuid()
    {
        // Act
        var entityId = GuidEntityId.Create();

        // Assert
        Assert.NotEqual(Guid.Empty, entityId.Value);
    }

    [Fact]
    public void Create_GeneratesUniqueValues()
    {
        // Act
        var id1 = GuidEntityId.Create();
        var id2 = GuidEntityId.Create();

        // Assert
        Assert.NotEqual(id1, id2);
    }
} 