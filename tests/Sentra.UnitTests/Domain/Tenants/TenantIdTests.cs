using FluentAssertions;
using Sentra.Domain.Tenants;

namespace Sentra.UnitTests.Domain.Tenants;

/// <summary>
/// Contains unit tests for <see cref="TenantId"/>.
/// </summary>
public sealed class TenantIdTests
{
    /// <summary>
    /// Verifies that <see cref="TenantId.New"/> creates a non-empty identifier.
    /// </summary>
    [Fact]
    public void New_Should_Create_NonEmpty_Id()
    {
        TenantId tenantId = TenantId.New();

        tenantId.Value.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Verifies that <see cref="TenantId.ToString"/> returns the underlying GUID string.
    /// </summary>
    [Fact]
    public void ToString_Should_Return_Guid_Value()
    {
        Guid value = Guid.Parse("11111111-1111-1111-1111-111111111111");
        TenantId tenantId = new TenantId(value);

        tenantId.ToString().Should().Be("11111111-1111-1111-1111-111111111111");
    }
}