using FluentAssertions;
using Sentra.SharedKernel.Abstractions;

namespace Sentra.UnitTests.SharedKernel.Abstractions;

/// <summary>
/// Contains unit tests for <see cref="Entity{TId}"/> equality behavior.
/// </summary>
public sealed class EntityTests
{
    /// <summary>
    /// Verifies that entities with the same type and identifier are equal.
    /// </summary>
    [Fact]
    public void Entities_With_Same_Type_And_Id_Should_Be_Equal()
    {
        TestEntity? left = new TestEntity(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        TestEntity? right = new TestEntity(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        left.Should().Be(right);
        (left == right).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that entities with different identifiers are not equal.
    /// </summary>
    [Fact]
    public void Entities_With_Different_Id_Should_Not_Be_Equal()
    {
        TestEntity? left = new TestEntity(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        TestEntity? right = new TestEntity(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        left.Should().NotBe(right);
        (left != right).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that entities with the same identifier but a different runtime type are not equal.
    /// </summary>
    [Fact]
    public void Entities_With_Same_Id_But_Different_Type_Should_Not_Be_Equal()
    {
        Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        TestEntity? left = new TestEntity(id);
        OtherTestEntity? right = new OtherTestEntity(id);

        left.Equals(right).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that equal entities produce the same hash code.
    /// </summary>
    [Fact]
    public void Equal_Entities_Should_Have_Same_HashCode()
    {
        Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        TestEntity? left = new TestEntity(id);
        TestEntity? right = new TestEntity(id);

        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    /// <summary>
    /// Test helper entity.
    /// </summary>
    private sealed class TestEntity : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEntity"/> class.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public TestEntity(Guid id)
            : base(id)
        {
        }
    }

    /// <summary>
    /// Alternative test helper entity.
    /// </summary>
    private sealed class OtherTestEntity : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OtherTestEntity"/> class.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public OtherTestEntity(Guid id)
            : base(id)
        {
        }
    }
}