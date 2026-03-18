using Microsoft.AspNetCore.Mvc;
using Sentra.Api.Models;
using Sentra.Api.Models.Chat;
using Sentra.Application.Abstractions.AI;
using Sentra.SharedKernel.Results;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides chat-based endpoints for Sentra AI operations.
/// </summary>
[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    /// <summary>
    /// Asks a natural-language question against a data source.
    /// </summary>
    /// <param name="request">The incoming ask-question request.</param>
    /// <param name="service">The ask-question service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An HTTP response containing either the ask-question response
    /// or a standardized error response.
    /// </returns>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AskQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask(
        [FromBody] AskQuestionRequest request,
        [FromServices] IAskQuestionService service,
        CancellationToken cancellationToken)
    {
        Result<AskQuestionResult>? result = await service.AskAsync(
            request.DataSourceType,
            request.ConnectionString,
            request.Question,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        AskQuestionResult? value = result.ValueOrThrow();

        return Ok(new AskQuestionResponse(
            value.Question,
            value.GeneratedSql,
            value.QueryResult.RowCount,
            value.Answer));
    }
}