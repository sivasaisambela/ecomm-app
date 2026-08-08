using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminService.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public string Action { get; private set; } = string.Empty;
        public string PerformedBy { get; private set; } = string.Empty;
        public string? EntityType { get; private set; }
        public string? EntityId { get; private set; }
        public string? Details { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private AuditLog() { }

        public AuditLog(
            Guid id,
            string action,
            string performedBy,
            string? entityType = null,
            string? entityId = null,
            string? details = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be empty.", nameof(action));

            if (string.IsNullOrWhiteSpace(performedBy))
                throw new ArgumentException("PerformedBy cannot be empty.", nameof(performedBy));

            Id = id;
            Action = action.Trim();
            PerformedBy = performedBy.Trim();
            EntityType = entityType?.Trim();
            EntityId = entityId?.Trim();
            Details = details?.Trim();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
