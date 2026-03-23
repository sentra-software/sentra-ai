using System.Text.RegularExpressions;
using Sentra.AI.Abstractions.Sql;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;

namespace Sentra.AI.Orchestration.Sql;

/// <summary>
/// Represents a schema-aware SQL identifier normalizer.
/// </summary>
public sealed class SqlIdentifierNormalizer : ISqlIdentifierNormalizer
{
    private static readonly Regex FromJoinRegex = new(
        @"\b(?<keyword>from|join)\s+(?:(?<schema>[A-Za-z_][A-Za-z0-9_]*)\.)?(?<table>[A-Za-z_][A-Za-z0-9_]*)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Normalizes table identifiers in the provided SQL query using the available schema metadata.
    /// </summary>
    /// <param name="dataSourceType">The target data source type.</param>
    /// <param name="sql">The generated SQL query.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <returns>The normalized SQL query.</returns>
    public string Normalize(
        DataSourceType dataSourceType,
        string sql,
        IReadOnlyCollection<TableSchema> tables)
    {
        if (string.IsNullOrWhiteSpace(sql) || tables is null || tables.Count == 0)
        {
            return sql;
        }

        Dictionary<string, TableSchema> tableMap = tables.ToDictionary(
            table => $"{table.Schema}.{table.Name}".ToLowerInvariant(),
            table => table);

        Dictionary<string, TableSchema[]> tableNameMap = tables
            .GroupBy(table => table.Name.ToLowerInvariant())
            .ToDictionary(group => group.Key, group => group.ToArray());

        string normalizedSql = FromJoinRegex.Replace(sql, match =>
        {
            string keyword = match.Groups["keyword"].Value;
            string? rawSchema = match.Groups["schema"].Success
                ? match.Groups["schema"].Value
                : null;
            string rawTable = match.Groups["table"].Value;

            if (!string.IsNullOrWhiteSpace(rawSchema))
            {
                string qualifiedLookupKey = $"{rawSchema}.{rawTable}".ToLowerInvariant();

                if (tableMap.TryGetValue(qualifiedLookupKey, out TableSchema? exactQualifiedTable))
                {
                    return $"{keyword} {FormatQualifiedTableName(dataSourceType, exactQualifiedTable)}";
                }
            }

            if (tableNameMap.TryGetValue(rawTable.ToLowerInvariant(), out TableSchema[]? candidates) &&
                candidates.Length == 1)
            {
                TableSchema exactTable = candidates[0];
                return $"{keyword} {FormatQualifiedTableName(dataSourceType, exactTable)}";
            }

            return match.Value;
        });

        return normalizedSql;
    }

    /// <summary>
    /// Formats a qualified table name for the selected dialect.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="table">The table schema metadata.</param>
    /// <returns>The formatted qualified table name.</returns>
    private static string FormatQualifiedTableName(DataSourceType dataSourceType, TableSchema table)
    {
        return dataSourceType switch
        {
            DataSourceType.MySql => $"`{table.Schema}`.`{table.Name}`",
            DataSourceType.Sqlite => $"\"{table.Name}\"",
            DataSourceType.SqlServer => $"[{table.Schema}].[{table.Name}]",
            _ => $@"{table.Schema}.""{table.Name}"""
        };
    }
}