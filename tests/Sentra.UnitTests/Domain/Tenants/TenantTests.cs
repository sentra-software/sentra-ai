using FluentAssertions;
using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Domain.Tenants;

/// <summary>
/// Contains unit tests for <see cref="Tenant"/>.
/// </summary>
public sealed class TenantTests
{
    /// <summary>
    /// Verifies that a tenant can be created with a valid name.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Success_When_Name_Is_Valid()
    {
        Result<Tenant>? result = Tenant.Create("Sentra Software");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.ValueOrThrow().Name.Should().Be("Sentra Software");
        result.ValueOrThrow().Status.Should().Be(TenantStatus.Active);
        result.ValueOrThrow().Id.Value.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Verifies that the name is trimmed during creation.
    /// </summary>
    [Fact]
    public void Create_Should_Trim_Name()
    {
        Result<Tenant>? result = Tenant.Create("  Sentra Software  ");

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().Name.Should().Be("Sentra Software");
    }

    /// <summary>
    /// Verifies that tenant creation fails when the name is empty.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_Name_Is_Empty()
    {
        Result<Tenant>? result = Tenant.Create("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("tenants.name.required");
    }

    /// <summary>
    /// Verifies that tenant creation fails when the name is too short.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_Name_Is_Too_Short()
    {
        Result<Tenant>? result = Tenant.Create("A");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("tenants.name.too_short");
    }

    /// <summary>
    /// Verifies that tenant creation fails when the name is too long.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_Name_Is_Too_Long()
    {
        string? name = new string('A', 201);

        Result<Tenant>? result = Tenant.Create(name);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("tenants.name.too_long");
    }

    /// <summary>
    /// Verifies that a tenant can be renamed with a valid name.
    /// </summary>
    [Fact]
    public void Rename_Should_Update_Name_When_Value_Is_Valid()
    {
        Tenant? tenant = Tenant.Create("Sentra Software").ValueOrThrow();

        Result? result = tenant.Rename("Sentra AI");

        result.IsSuccess.Should().BeTrue();
        tenant.Name.Should().Be("Sentra AI");
    }

    /// <summary>
    /// Verifies that renaming trims the provided value.
    /// </summary>
    [Fact]
    public void Rename_Should_Trim_Name()
    {
        Tenant? tenant = Tenant.Create("Sentra Software").ValueOrThrow();

        Result? result = tenant.Rename("  Sentra AI  ");

        result.IsSuccess.Should().BeTrue();
        tenant.Name.Should().Be("Sentra AI");
    }

    /// <summary>
    /// Verifies that renaming fails when the provided value is invalid.
    /// </summary>
    [Fact]
    public void Rename_Should_Return_Failure_When_Name_Is_Invalid()
    {
        Tenant? tenant = Tenant.Create("Sentra Software").ValueOrThrow();

        Result? result = tenant.Rename(" ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("tenants.name.required");
        tenant.Name.Should().Be("Sentra Software");
    }

    /// <summary>
    /// Verifies that a tenant can be deactivated.
    /// </summary>
    [Fact]
    public void Deactivate_Should_Set_Status_To_Inactive()
    {
        Tenant? tenant = Tenant.Create("Sentra Software").ValueOrThrow();

        tenant.Deactivate();

        tenant.Status.Should().Be(TenantStatus.Inactive);
    }

    /// <summary>
    /// Verifies that a tenant can be activated after deactivation.
    /// </summary>
    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        Tenant? tenant = Tenant.Create("Sentra Software").ValueOrThrow();
        tenant.Deactivate();

        tenant.Activate();

        tenant.Status.Should().Be(TenantStatus.Active);
    }
}