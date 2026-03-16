using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Users;

/// <summary>
/// Represents a user that belongs to a tenant in the Sentra platform.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="email">The normalized email address.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="role">The assigned user role.</param>
    private User(
        UserId id,
        TenantId tenantId,
        string email,
        string displayName,
        UserRole role)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        DisplayName = displayName;
        Role = role;
    }

    /// <summary>
    /// Gets the tenant identifier the user belongs to.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Gets the normalized email address of the user.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the assigned role of the user.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Creates a new user instance.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="email">The email address.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="role">The user role.</param>
    /// <returns>
    /// A successful result containing the created <see cref="User"/>,
    /// or a failed result when validation fails.
    /// </returns>
    public static Result<User> Create(
        TenantId tenantId,
        string email,
        string displayName,
        UserRole role)
    {
        Result? tenantValidationResult = ValidateTenantId(tenantId);
        if (tenantValidationResult.IsFailure)
        {
            return Result.Failure<User>(tenantValidationResult.Error);
        }

        Result? emailValidationResult = ValidateEmail(email);
        if (emailValidationResult.IsFailure)
        {
            return Result.Failure<User>(emailValidationResult.Error);
        }

        Result? displayNameValidationResult = ValidateDisplayName(displayName);
        if (displayNameValidationResult.IsFailure)
        {
            return Result.Failure<User>(displayNameValidationResult.Error);
        }

        Result? roleValidationResult = ValidateRole(role);
        if (roleValidationResult.IsFailure)
        {
            return Result.Failure<User>(roleValidationResult.Error);
        }

        User? user = new User(
            UserId.New(),
            tenantId,
            NormalizeEmail(email),
            displayName.Trim(),
            role);

        return Result.Success(user);
    }

    /// <summary>
    /// Changes the display name of the user.
    /// </summary>
    /// <param name="displayName">The new display name.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    public Result Rename(string displayName)
    {
        Result? validationResult = ValidateDisplayName(displayName);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        DisplayName = displayName.Trim();

        return Result.Success();
    }

    /// <summary>
    /// Changes the email address of the user.
    /// </summary>
    /// <param name="email">The new email address.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    public Result ChangeEmail(string email)
    {
        Result? validationResult = ValidateEmail(email);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Email = NormalizeEmail(email);

        return Result.Success();
    }

    /// <summary>
    /// Changes the role of the user.
    /// </summary>
    /// <param name="role">The new user role.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    public Result ChangeRole(UserRole role)
    {
        Result? validationResult = ValidateRole(role);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Role = role;

        return Result.Success();
    }

    /// <summary>
    /// Validates the tenant identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>A result indicating whether the value is valid.</returns>
    private static Result ValidateTenantId(TenantId tenantId)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure(
                Error.Validation(
                    "users.tenant_id.required",
                    "Tenant identifier is required."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates the email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>A result indicating whether the value is valid.</returns>
    private static Result ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure(
                Error.Validation(
                    "users.email.required",
                    "User email is required."));
        }

        string? trimmedEmail = email.Trim();

        if (trimmedEmail.Length > 320)
        {
            return Result.Failure(
                Error.Validation(
                    "users.email.too_long",
                    "User email must not exceed 320 characters."));
        }

        if (!trimmedEmail.Contains('@'))
        {
            return Result.Failure(
                Error.Validation(
                    "users.email.invalid",
                    "User email must contain an at-sign."));
        }

        string[]? parts = trimmedEmail.Split('@', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return Result.Failure(
                Error.Validation(
                    "users.email.invalid",
                    "User email format is invalid."));
        }

        if (!parts[1].Contains('.'))
        {
            return Result.Failure(
                Error.Validation(
                    "users.email.invalid",
                    "User email domain format is invalid."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates the display name.
    /// </summary>
    /// <param name="displayName">The display name.</param>
    /// <returns>A result indicating whether the value is valid.</returns>
    private static Result ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(
                Error.Validation(
                    "users.display_name.required",
                    "User display name is required."));
        }

        string? trimmedDisplayName = displayName.Trim();

        if (trimmedDisplayName.Length < 2)
        {
            return Result.Failure(
                Error.Validation(
                    "users.display_name.too_short",
                    "User display name must be at least 2 characters long."));
        }

        if (trimmedDisplayName.Length > 200)
        {
            return Result.Failure(
                Error.Validation(
                    "users.display_name.too_long",
                    "User display name must not exceed 200 characters."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates the user role.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <returns>A result indicating whether the value is valid.</returns>
    private static Result ValidateRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            return Result.Failure(
                Error.Validation(
                    "users.role.invalid",
                    "User role is invalid."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Normalizes an email address for consistent storage and comparison.
    /// </summary>
    /// <param name="email">The raw email address.</param>
    /// <returns>The normalized email address.</returns>
    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}