using Microsoft.AspNetCore.DataProtection;
using Sentra.Application.Abstractions.Security;

namespace Sentra.Infrastructure.Security;

/// <summary>
/// Protects connection strings using ASP.NET Core Data Protection.
/// </summary>
public sealed class DataProtectionConnectionStringProtector : IConnectionStringProtector
{
    private const string Purpose = "Sentra.ManagedDataSources.ConnectionStrings";
    private readonly IDataProtector _protector;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataProtectionConnectionStringProtector"/> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    public DataProtectionConnectionStringProtector(IDataProtectionProvider dataProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    /// <inheritdoc />
    public string Protect(string plainTextConnectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainTextConnectionString);
        return _protector.Protect(plainTextConnectionString);
    }

    /// <inheritdoc />
    public string Unprotect(string protectedConnectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedConnectionString);
        return _protector.Unprotect(protectedConnectionString);
    }
}