using SSO.Domain.Entities.Branches.Events;
using SSO.Domain.Entities.Users;

namespace SSO.Domain.Entities.Branches
{
    public partial class Branch
    {
        public Branch()
        {
            Users = new HashSet<User>();
        }

        public Branch(string name
            , string description) : this()
        {
            Update(name, description);
        }

        public void Update(string name
            , string description)
        {
            Name = name;
            Description = description;
        }
        public Branch Register(Branch branch)
        {
            var addEvent = new OnRegisteredEvent()
            {
                Branch = branch
            };
            AddEvent(addEvent);
            return branch;
        }
    }
}