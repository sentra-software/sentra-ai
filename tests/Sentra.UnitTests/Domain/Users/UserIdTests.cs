using FluentAssertions;
using Sentra.Domain.Users;

namespace Sentra.UnitTests.Domain.Users;

/// <summary>
/// Contains unit tests for <see cref="UserId"/>.
/// </summary>
public sealed class UserIdTests
{
    /// <summary>
    /// Verifies that <see cref="UserId.New"/> creates a non-empty identifier.
    /// </summary>
    [Fact]
    public void New_Should_Create_NonEmpty_Id()
    {
        UserId userId = UserId.New();

        userId.Value.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Verifies that <see cref="UserId.ToString"/> returns the underlying GUID string.
    /// </summary>
    [Fact]
    public void ToString_Should_Return_Guid_Value()
    {
        Guid value = Guid.Parse("11111111-1111-1111-1111-111111111111");
        UserId userId = new UserId(value);

        userId.ToString().Should().Be("11111111-1111-1111-1111-111111111111");
    }
}