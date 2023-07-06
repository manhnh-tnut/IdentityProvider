using SSO.Domain.Base;

namespace SSO.Domain.Entities.AuditLogs
{
    public class AuditLog : BaseEntity<Guid>
    {
        public AuditLog() : base() { }
        public string Event { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public string SubjectIdentifier { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }
        public string SubjectAdditionalData { get; set; }
        public string Action { get; set; }
        public string Data { get; set; }
    }
}
