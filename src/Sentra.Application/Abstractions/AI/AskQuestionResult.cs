using Sentra.Connectors.Abstractions.Querying;

namespace Sentra.Application.Abstractions.AI;

/// <summary>
/// Represents the result of an ask question operation.
/// </summary>
public sealed class AskQuestionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AskQuestionResult"/> class.
    /// </summary>
    public AskQuestionResult(
        string question,
        string generatedSql,
        QueryExecutionResult queryResult,
        string answer
    )
    {
        Question = question;
        GeneratedSql = generatedSql;
        QueryResult = queryResult;
        Answer = answer;
    }
    
    /// <summary>
    /// Gets the original user question.
    /// </summary>
    public string Question { get; }

    /// <summary>
    /// Gets the generated SQL query.
    /// </summary>
    public string GeneratedSql { get; }

    /// <summary>
    /// Gets the query execution result.
    /// </summary>
    public QueryExecutionResult QueryResult { get; }

    /// <summary>
    /// Gets the generated natural-language answer.
    /// </summary>
    public string Answer { get; }
}