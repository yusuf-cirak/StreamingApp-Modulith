using Xunit;

namespace BuildingBlocks.Domain.UnitTests;

public class ValueObjectTests
{
    private record TestValueObject : ValueObject
    {
        public string Value1 { get; }
        public int Value2 { get; }

        public TestValueObject(string value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value1;
            yield return Value2;
        }
    }

    [Fact]
    public void Equals_ReturnsTrueForSameValues()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 1);
        var obj2 = new TestValueObject("test", 1);

        // Act & Assert
        Assert.True(obj1.Equals(obj2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 1);
        var obj2 = new TestValueObject("test", 2);

        // Act & Assert
        Assert.False(obj1.Equals(obj2));
    }

    [Fact]
    public void Equals_ReturnsFalseForNull()
    {
        // Arrange
        var obj = new TestValueObject("test", 1);

        // Act & Assert
        Assert.False(obj.Equals(null));
    }

    [Fact]
    public void GetHashCode_ReturnsSameValueForEqualObjects()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 1);
        var obj2 = new TestValueObject("test", 1);

        // Act & Assert
        Assert.Equal(obj1.GetHashCode(), obj2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ReturnsDifferentValueForDifferentObjects()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 1);
        var obj2 = new TestValueObject("test", 2);

        // Act & Assert
        Assert.NotEqual(obj1.GetHashCode(), obj2.GetHashCode());
    }
} 