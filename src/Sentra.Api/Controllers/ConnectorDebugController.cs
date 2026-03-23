using Microsoft.AspNetCore.Mvc;
using Sentra.Connectors.Abstractions.Connectors;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides temporary debug endpoints for testing registered data connectors.
/// Remove this controller before production release or protect it properly.
/// </summary>
[ApiController]
[Route("api/debug/connectors")]
public sealed class ConnectorDebugController : ControllerBase
{
    private readonly IReadOnlyDictionary<ConnectorType, IDataConnector> connectors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorDebugController"/> class.
    /// </summary>
    /// <param name="connectors">The registered data connectors.</param>
    public ConnectorDebugController(IEnumerable<IDataConnector> connectors)
    {
        ArgumentNullException.ThrowIfNull(connectors);

        this.connectors = connectors.ToDictionary(x => x.Type);
    }

    /// <summary>
    /// Gets all registered connector types.
    /// </summary>
    /// <returns>The registered connector metadata.</returns>
    [HttpGet]
    public ActionResult<IReadOnlyCollection<object>> GetRegisteredConnectors()
    {
        IReadOnlyCollection<object> result = connectors
            .Values
            .Select(x => new
            {
                Type = x.Type.ToString(),
                Capabilities = x.Capabilities.Select(c => c.ToString()).ToArray()
            })
            .ToArray();

        return Ok(result);
    }

    /// <summary>
    /// Tests a connection string against a specific connector type.
    /// </summary>
    /// <param name="request">The connection test request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection test result.</returns>
    [HttpPost("test-connection")]
    public async Task<ActionResult<object>> TestConnectionAsync(
        [FromBody] ConnectorDebugTestConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!connectors.TryGetValue(request.ConnectorType, out IDataConnector? connector))
        {
            return NotFound(new
            {
                Message = $"No connector is registered for type '{request.ConnectorType}'."
            });
        }

        ConnectionTestResult result = await connector.TestConnectionAsync(
            request.ConnectionString,
            cancellationToken);

        return Ok(new
        {
            ConnectorType = request.ConnectorType.ToString(),
            result.IsSuccess,
            result.Message
        });
    }

    /// <summary>
    /// Reads the schema for a specific connector and connection string.
    /// </summary>
    /// <param name="request">The schema read request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered schema.</returns>
    [HttpPost("read-schema")]
    public async Task<ActionResult<object>> ReadSchemaAsync(
        [FromBody] ConnectorDebugReadSchemaRequest request,
        CancellationToken cancellationToken)
    {
        if (!connectors.TryGetValue(request.ConnectorType, out IDataConnector? connector))
        {
            return NotFound(new
            {
                Message = $"No connector is registered for type '{request.ConnectorType}'."
            });
        }

        IReadOnlyCollection<Sentra.Connectors.Abstractions.Schema.TableSchema> schema =
            await connector.ReadSchemaAsync(
                request.ConnectionString,
                cancellationToken);

        object result = schema.Select(table => new
        {
            table.Schema,
            table.Name,
            Columns = table.Columns.Select(column => new
            {
                column.Name,
                column.DataType,
                column.IsNullable
            }).ToArray()
        });

        return Ok(result);
    }

    /// <summary>
    /// Executes a query for a specific connector and connection string.
    /// </summary>
    /// <param name="request">The query execution request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query execution result.</returns>
    [HttpPost("execute")]
    public async Task<ActionResult<object>> ExecuteAsync(
        [FromBody] ConnectorDebugExecuteQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (!connectors.TryGetValue(request.ConnectorType, out IDataConnector? connector))
        {
            return NotFound(new
            {
                Message = $"No connector is registered for type '{request.ConnectorType}'."
            });
        }

        Sentra.Connectors.Abstractions.Querying.QueryExecutionResult result =
            await connector.ExecuteQueryAsync(
                request.ConnectionString,
                request.Query,
                cancellationToken);

        return Ok(new
        {
            result.RowCount,
            result.Columns,
            result.Rows
        });
    }
}

/// <summary>
/// Represents a debug request for testing a connector connection.
/// </summary>
/// <param name="ConnectorType">The connector type.</param>
/// <param name="ConnectionString">The connection string.</param>
public sealed record ConnectorDebugTestConnectionRequest(
    ConnectorType ConnectorType,
    string ConnectionString);

/// <summary>
/// Represents a debug request for reading schema.
/// </summary>
/// <param name="ConnectorType">The connector type.</param>
/// <param name="ConnectionString">The connection string.</param>
public sealed record ConnectorDebugReadSchemaRequest(
    ConnectorType ConnectorType,
    string ConnectionString);

/// <summary>
/// Represents a debug request for executing a query.
/// </summary>
/// <param name="ConnectorType">The connector type.</param>
/// <param name="ConnectionString">The connection string.</param>
/// <param name="Query">The SQL query.</param>
public sealed record ConnectorDebugExecuteQueryRequest(
    ConnectorType ConnectorType,
    string ConnectionString,
    string Query);