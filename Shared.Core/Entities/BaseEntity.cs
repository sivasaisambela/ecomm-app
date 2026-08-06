namespace Shared.Core.Entities;

/// <summary>
/// Base class for all domain entities.
///
/// Provides common audit + concurrency fields so every entity
/// (Product, Order, etc.) doesn't have to redeclare them.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft-delete flag. Entities are never physically removed.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Optimistic concurrency token (maps to SQL Server rowversion/timestamp).
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
