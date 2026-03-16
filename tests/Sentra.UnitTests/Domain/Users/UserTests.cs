using FluentAssertions;
using Sentra.Domain.Tenants;
using Sentra.Domain.Users;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Domain.Users;

/// <summary>
/// Contains unit tests for <see cref="User"/>.
/// </summary>
public sealed class UserTests
{
    /// <summary>
    /// Verifies that a user can be created with valid values.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Success_When_Values_Are_Valid()
    {
        TenantId tenantId = TenantId.New();

        Result<User>? result = User.Create(
            tenantId,
            "Josey@Sentra.ai",
            "Josey",
            UserRole.Owner);

        result.IsSuccess.Should().BeTrue();

        User? user = result.ValueOrThrow();
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.TenantId.Should().Be(tenantId);
        user.Email.Should().Be("josey@sentra.ai");
        user.DisplayName.Should().Be("Josey");
        user.Role.Should().Be(UserRole.Owner);
    }

    /// <summary>
    /// Verifies that user creation fails when the tenant identifier is empty.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_TenantId_Is_Empty()
    {
        Result<User>? result = User.Create(
            new TenantId(Guid.Empty),
            "josey@sentra.ai",
            "Josey",
            UserRole.Owner);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.tenant_id.required");
    }

    /// <summary>
    /// Verifies that user creation fails when the email is empty.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_Email_Is_Empty()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            " ",
            "Josey",
            UserRole.Owner);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.email.required");
    }

    /// <summary>
    /// Verifies that user creation fails when the email format is invalid.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_Email_Is_Invalid()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            "invalid-email",
            "Josey",
            UserRole.Owner);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.email.invalid");
    }

    /// <summary>
    /// Verifies that user creation fails when the display name is empty.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_DisplayName_Is_Empty()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            " ",
            UserRole.Owner);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.display_name.required");
    }

    /// <summary>
    /// Verifies that user creation fails when the display name is too short.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_DisplayName_Is_Too_Short()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "J",
            UserRole.Owner);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.display_name.too_short");
    }

    /// <summary>
    /// Verifies that user creation fails when the role is invalid.
    /// </summary>
    [Fact]
    public void Create_Should_Return_Failure_When_Role_Is_Invalid()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            (UserRole)999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.role.invalid");
    }

    /// <summary>
    /// Verifies that the email is normalized during creation.
    /// </summary>
    [Fact]
    public void Create_Should_Normalize_Email()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            "  Josey@Sentra.AI  ",
            "Josey",
            UserRole.Owner);

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().Email.Should().Be("josey@sentra.ai");
    }

    /// <summary>
    /// Verifies that the display name is trimmed during creation.
    /// </summary>
    [Fact]
    public void Create_Should_Trim_DisplayName()
    {
        Result<User>? result = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "  Josey  ",
            UserRole.Owner);

        result.IsSuccess.Should().BeTrue();
        result.ValueOrThrow().DisplayName.Should().Be("Josey");
    }

    /// <summary>
    /// Verifies that a user can be renamed.
    /// </summary>
    [Fact]
    public void Rename_Should_Update_DisplayName_When_Value_Is_Valid()
    {
        User? user = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            UserRole.Member).ValueOrThrow();

        Result? result = user.Rename("Josey Martinus");

        result.IsSuccess.Should().BeTrue();
        user.DisplayName.Should().Be("Josey Martinus");
    }

    /// <summary>
    /// Verifies that a rename failure does not change the current display name.
    /// </summary>
    [Fact]
    public void Rename_Should_Return_Failure_When_Value_Is_Invalid()
    {
        User? user = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            UserRole.Member).ValueOrThrow();

        Result? result = user.Rename(" ");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.display_name.required");
        user.DisplayName.Should().Be("Josey");
    }

    /// <summary>
    /// Verifies that a user email can be changed and normalized.
    /// </summary>
    [Fact]
    public void ChangeEmail_Should_Update_Email_When_Value_Is_Valid()
    {
        User? user = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            UserRole.Member).ValueOrThrow();

        Result? result = user.ChangeEmail("  Contact@Sentra.ai ");

        result.IsSuccess.Should().BeTrue();
        user.Email.Should().Be("contact@sentra.ai");
    }

    /// <summary>
    /// Verifies that an invalid email change is rejected.
    /// </summary>
    [Fact]
    public void ChangeEmail_Should_Return_Failure_When_Value_Is_Invalid()
    {
        User? user = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            UserRole.Member).ValueOrThrow();

        Result? result = user.ChangeEmail("invalid-email");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.email.invalid");
        user.Email.Should().Be("josey@sentra.ai");
    }

    /// <summary>
    /// Verifies that a valid role change is applied.
    /// </summary>
    [Fact]
    public void ChangeRole_Should_Update_Role_When_Value_Is_Valid()
    {
        User? user = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            UserRole.Member).ValueOrThrow();

        Result? result = user.ChangeRole(UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
        user.Role.Should().Be(UserRole.Admin);
    }

    /// <summary>
    /// Verifies that an invalid role change is rejected.
    /// </summary>
    [Fact]
    public void ChangeRole_Should_Return_Failure_When_Value_Is_Invalid()
    {
        User? user = User.Create(
            TenantId.New(),
            "josey@sentra.ai",
            "Josey",
            UserRole.Member).ValueOrThrow();

        Result? result = user.ChangeRole((UserRole)999);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("users.role.invalid");
        user.Role.Should().Be(UserRole.Member);
    }
}