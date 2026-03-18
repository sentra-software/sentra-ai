using Microsoft.AspNetCore.Mvc;
using Sentra.Api.Models;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Contracts.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides endpoints for data source operations such as connection testing,
/// schema discovery, and query execution.
/// </summary>
[ApiController]
[Route("api/data-sources")]
public sealed class DataSourcesController : ControllerBase
{
    /// <summary>
    /// Tests a data source connection.
    /// </summary>
    /// <param name="request">The incoming connection test request.</param>
    /// <param name="service">The data source connection service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An HTTP response containing either the connection test response
    /// or a standardized error response.
    /// </returns>
    [HttpPost("test-connection")]
    [ProducesResponseType(typeof(TestDataSourceConnectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestConnection(
        [FromBody] TestDataSourceConnectionRequest request,
        [FromServices] IDataSourceConnectionService service,
        CancellationToken cancellationToken)
    {
        Result<ConnectionTestResult>? result = await service.TestConnectionAsync(
            request.DataSourceType,
            request.ConnectionString,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        return Ok(result.ValueOrThrow().ToResponse());
    }

    /// <summary>
    /// Reads the schema of a data source.
    /// </summary>
    /// <param name="request">The incoming schema read request.</param>
    /// <param name="service">The data source schema service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An HTTP response containing either the schema response
    /// or a standardized error response.
    /// </returns>
    [HttpPost("read-schema")]
    [ProducesResponseType(typeof(ReadDataSourceSchemaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReadSchema(
        [FromBody] ReadDataSourceSchemaRequest request,
        [FromServices] IDataSourceSchemaService service,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<TableSchema>>? result = await service.ReadSchemaAsync(
            request.DataSourceType,
            request.ConnectionString,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        return Ok(result.ValueOrThrow().ToResponse());
    }

    /// <summary>
    /// Executes a query against a data source.
    /// </summary>
    /// <param name="request">The incoming query execution request.</param>
    /// <param name="service">The data source query service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An HTTP response containing either the query execution response
    /// or a standardized error response.
    /// </returns>
    [HttpPost("execute-query")]
    [ProducesResponseType(typeof(ExecuteDataSourceQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteQuery(
        [FromBody] ExecuteDataSourceQueryRequest request,
        [FromServices] IDataSourceQueryService service,
        CancellationToken cancellationToken)
    {
        Result<QueryExecutionResult>? result = await service.ExecuteQueryAsync(
            request.DataSourceType,
            request.ConnectionString,
            request.Query,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        return Ok(result.ValueOrThrow().ToResponse());
    }
}