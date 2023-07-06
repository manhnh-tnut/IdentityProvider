using SSO.Domain.Base;
using SSO.Domain.Entities.Roles.Events;
using SSO.Domain.Entities.Users;

namespace SSO.Domain.Entities.Roles
{
    public partial class Role : IAggregateRoot
    {
        public Role(string name) : base(name)
        {
            this.Update(name);
        }

        public void Update(string name)
        {
            Name = name;
        }

        public Role Register(Role role)
        {
            var addEvent = new OnRegisteredEvent()
            {
                Role = role
            };
            AddEvent(addEvent);
            return role;
        }
    }
}