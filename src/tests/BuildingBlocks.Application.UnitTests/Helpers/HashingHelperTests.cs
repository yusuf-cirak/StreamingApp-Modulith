using Xunit;
using BuildingBlocks.Application.Abstractions.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace BuildingBlocks.Application.UnitTests.Helpers;

public class HashingHelperTests
{
    private class TestHashingHelper : IHashingHelper
    {
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }

    private readonly IHashingHelper _hashingHelper;

    public HashingHelperTests()
    {
        _hashingHelper = new TestHashingHelper();
    }

    [Fact]
    public void CreatePasswordHash_GeneratesUniqueHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        _hashingHelper.CreatePasswordHash(password, out byte[] hash1, out byte[] salt1);
        _hashingHelper.CreatePasswordHash(password, out byte[] hash2, out byte[] salt2);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotNull(salt1);
        Assert.NotEmpty(hash1);
        Assert.NotEmpty(salt1);
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsTrueForCorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        _hashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = _hashingHelper.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPasswordHash_ReturnsFalseForIncorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        _hashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        // Act
        var result = _hashingHelper.VerifyPasswordHash(wrongPassword, hash, salt);

        // Assert
        Assert.False(result);
    }
} 