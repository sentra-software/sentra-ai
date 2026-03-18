using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Sentra.Api.Controllers;
using Sentra.Api.Models;
using Sentra.Api.Models.Chat;
using Sentra.Application.Abstractions.AI;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Contains unit tests for <see cref="ChatController"/>.
/// </summary>
public sealed class ChatControllerTests
{
    /// <summary>
    /// Verifies that <see cref="ChatController.Ask"/> returns OK for a successful result.
    /// </summary>
    [Fact]
    public async Task Ask_Should_Return_Ok_When_Service_Succeeds()
    {
        var askResult = new AskQuestionResult(
            "Show me companies",
            "select \"Id\", \"Name\" from \"Companies\" limit 10;",
            new QueryExecutionResult(
                new[] { "Id", "Name" },
                new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["Id"] = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        ["Name"] = "Pulse"
                    }
                },
                1),
            "I found 1 row for your question.");

        IAskQuestionService service = new TestAskQuestionService(Result.Success(askResult));
        var controller = new ChatController();

        var request = new AskQuestionRequest(
            DataSourceType.PostgreSql,
            "Host=localhost;",
            "Show me companies");

        var result = await controller.Ask(request, service, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AskQuestionResponse>().Subject;

        response.Question.Should().Be("Show me companies");
        response.GeneratedSql.Should().Be("select \"Id\", \"Name\" from \"Companies\" limit 10;");
        response.RowCount.Should().Be(1);
        response.Answer.Should().Be("I found 1 row for your question.");
    }

    /// <summary>
    /// Verifies that <see cref="ChatController.Ask"/> returns BadRequest for a failed result.
    /// </summary>
    [Fact]
    public async Task Ask_Should_Return_BadRequest_When_Service_Fails()
    {
        IAskQuestionService service = new TestAskQuestionService(
            Result.Failure<AskQuestionResult>(
                Error.Validation("ai.question.required", "Question is required.")));

        var controller = new ChatController();

        var request = new AskQuestionRequest(
            DataSourceType.PostgreSql,
            "Host=localhost;",
            string.Empty);

        var result = await controller.Ask(request, service, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;

        response.Code.Should().Be("ai.question.required");
        response.Message.Should().Be("Question is required.");
    }
}