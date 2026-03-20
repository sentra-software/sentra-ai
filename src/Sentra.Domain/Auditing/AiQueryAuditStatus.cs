namespace Sentra.Domain.Auditing;

/// <summary>
/// Represents the outcome of an AI query execution.
/// </summary>
public enum AiQueryAuditStatus
{
    Succeeded = 1,
    Failed = 2,
    Blocked = 3
}