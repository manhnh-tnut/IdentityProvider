using SSO.Domain.Base;
using SSO.Domain.Entities.Branches;
using SSO.Domain.Entities.Users;

namespace SSO.Domain.Entities.Departments
{
    public partial class Department : BaseEntity<Guid>
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }

        public virtual ICollection<User> Users { get; internal set; }
        public virtual ICollection<Branch> Branches { get; internal set; }
    }
}