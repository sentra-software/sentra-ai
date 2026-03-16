using FluentAssertions;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Domain.DataSources;

/// <summary>
/// Contains unit tests for the <see cref="DataSource"/> domain entity.
/// </summary>
public sealed class DataSourceTests
{
    /// <summary>
    /// Verifies that a data source can be created successfully when all values are valid.
    /// </summary>
    [Fact]
    public void Create_Should_Succeed_With_Valid_Values()
    {
        // Arrange
        TenantId tenantId = TenantId.New();

        // Act
        Result<DataSource>? result = DataSource.Create(
            tenantId,
            "Main Database",
            DataSourceType.PostgreSql,
            "encrypted_connection_string");

        // Assert
        result.IsSuccess.Should().BeTrue();

        DataSource? ds = result.ValueOrThrow();

        ds.Id.Value.Should().NotBe(Guid.Empty);
        ds.Name.Should().Be("Main Database");
        ds.Type.Should().Be(DataSourceType.PostgreSql);
        ds.Status.Should().Be(DataSourceStatus.Pending);
    }

    /// <summary>
    /// Verifies that creation fails when the data source name is empty.
    /// </summary>
    [Fact]
    public void Create_Should_Fail_When_Name_Is_Empty()
    {
        // Act
        Result<DataSource>? result = DataSource.Create(
            TenantId.New(),
            " ",
            DataSourceType.PostgreSql,
            "encrypted_connection_string");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("datasource.name.required");
    }

    /// <summary>
    /// Verifies that the name of a data source can be changed successfully.
    /// </summary>
    [Fact]
    public void Rename_Should_Update_Name()
    {
        // Arrange
        DataSource? ds = DataSource.Create(
            TenantId.New(),
            "Old Name",
            DataSourceType.PostgreSql,
            "encrypted_connection_string")
            .ValueOrThrow();

        // Act
        Result? result = ds.Rename("New Name");

        // Assert
        result.IsSuccess.Should().BeTrue();
        ds.Name.Should().Be("New Name");
    }

    /// <summary>
    /// Verifies that activating the data source changes its status to active.
    /// </summary>
    [Fact]
    public void Activate_Should_Set_Status_To_Active()
    {
        // Arrange
        DataSource? ds = DataSource.Create(
            TenantId.New(),
            "Database",
            DataSourceType.PostgreSql,
            "encrypted_connection_string")
            .ValueOrThrow();

        // Act
        ds.Activate();

        // Assert
        ds.Status.Should().Be(DataSourceStatus.Active);
    }

    /// <summary>
    /// Verifies that disabling the data source changes its status to disabled.
    /// </summary>
    [Fact]
    public void Disable_Should_Set_Status_To_Disabled()
    {
        // Arrange
        DataSource? ds = DataSource.Create(
            TenantId.New(),
            "Database",
            DataSourceType.PostgreSql,
            "encrypted_connection_string")
            .ValueOrThrow();

        // Act
        ds.Disable();

        // Assert
        ds.Status.Should().Be(DataSourceStatus.Disabled);
    }

    /// <summary>
    /// Verifies that the encrypted connection string can be updated successfully.
    /// </summary>
    [Fact]
    public void UpdateConnectionString_Should_Update_Value()
    {
        // Arrange
        DataSource? ds = DataSource.Create(
            TenantId.New(),
            "Database",
            DataSourceType.PostgreSql,
            "encrypted_connection_string")
            .ValueOrThrow();

        // Act
        Result? result = ds.UpdateConnectionString("new_encrypted_connection_string");

        // Assert
        result.IsSuccess.Should().BeTrue();
        ds.EncryptedConnectionString.Should().Be("new_encrypted_connection_string");
    }
}