using SSO.Domain.Base;
using SSO.Domain.Entities.Departments;
using SSO.Domain.Entities.Users;

namespace SSO.Domain.Entities.Branches
{
    public partial class Branch : BaseEntity<Guid>
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }

        public virtual ICollection<User> Users { get; internal set; }
        public virtual ICollection<Department> Departments { get; internal set; }
    }
}