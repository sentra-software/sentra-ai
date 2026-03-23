using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;

namespace Sentra.AI.Abstractions.Sql;

/// <summary>
/// Defines a service that normalizes generated SQL identifiers against a known database schema.
/// </summary>
public interface ISqlIdentifierNormalizer
{
    /// <summary>
    /// Normalizes table identifiers in the provided SQL query using the available schema metadata.
    /// </summary>
    /// <param name="dataSourceType">The target data source type.</param>
    /// <param name="sql">The generated SQL query.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <returns>The normalized SQL query.</returns>
    string Normalize(
        DataSourceType dataSourceType,
        string sql,
        IReadOnlyCollection<TableSchema> tables);
}