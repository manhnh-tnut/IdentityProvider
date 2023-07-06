using Microsoft.AspNetCore.Identity;
using SSO.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSO.Domain.Entities.Roles
{
    public partial class Role : IdentityRole
    {
        public Role() : base()
        {
            Deleted = false;
            Created = DateTime.Now;
            _events = new List<BaseEvent>();
        }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Updated { get; set; }
        public string UpdatedBy { get; set; }
        public bool Deleted { get; set; }
        public bool Published { get; set; }

        [NotMapped]
        private readonly List<BaseEvent> _events;
        [NotMapped]
        public IReadOnlyList<BaseEvent> Events => _events.AsReadOnly();

        protected void AddEvent(BaseEvent @event)
        {
            _events?.Add(@event);
        }

        protected void RemoveEvent(BaseEvent @event)
        {
            _events?.Remove(@event);
        }

        public void ClearEvents()
        {
            _events?.Clear();
        }
    }
}
