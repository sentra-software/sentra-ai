namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents the response of a data source connection test.
/// </summary>
/// <param name="IsSuccess">Indicates whether the connection test succeeded.</param>
/// <param name="Message">The connection test message.</param>
public sealed record TestDataSourceConnectionResponse(
    bool IsSuccess,
    string Message);