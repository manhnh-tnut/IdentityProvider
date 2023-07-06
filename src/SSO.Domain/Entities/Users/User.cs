using Microsoft.AspNetCore.Identity;
using SSO.Domain.Base;
using SSO.Domain.Entities.Branches;
using SSO.Domain.Entities.Departments;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSO.Domain.Entities.Users
{
    public partial class User : IdentityUser
    {
        public User() : base()
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
        public string FullName { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool? Gender { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual Department Department { get; set; }
        public virtual ICollection<Avatar> Avatars { get; set; }

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