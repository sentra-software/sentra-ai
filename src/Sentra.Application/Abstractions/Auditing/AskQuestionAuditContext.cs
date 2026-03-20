namespace Sentra.Application.Abstractions.Auditing;

/// <summary>
/// Represents contextual information needed for AI query auditing.
/// </summary>
public sealed record AskQuestionAuditContext(
    Guid TenantId,
    Guid IdentityUserId,
    Guid DataSourceId);