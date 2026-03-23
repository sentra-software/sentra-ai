using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;

namespace Sentra.AI.Abstractions.Sql;

/// <summary>
/// Defines a service that attempts to resolve a natural-language question to a predefined SQL template.
/// </summary>
public interface ISqlTemplateMatcher
{
    /// <summary>
    /// Attempts to match the provided question to a predefined SQL template using the available schema.
    /// </summary>
    /// <param name="dataSourceType">The target data source type.</param>
    /// <param name="question">The user question.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <returns>
    /// A <see cref="SqlTemplateMatchResult"/> indicating whether a template match was found.
    /// </returns>
    SqlTemplateMatchResult Match(
        DataSourceType dataSourceType,
        string question,
        IReadOnlyCollection<TableSchema> tables);
}