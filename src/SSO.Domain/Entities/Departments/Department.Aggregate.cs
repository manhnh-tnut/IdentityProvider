using SSO.Domain.Entities.Departments.Events;
using SSO.Domain.Entities.Users;

namespace SSO.Domain.Entities.Departments
{
    public partial class Department
    {
        public Department()
        {
            Users = new HashSet<User>();
        }

        public Department(string name
            , string description) : this()
        {
            this.Update(name, description);
        }

        public void Update(string name
            , string description)
        {
            Name = name;
            Description = description;
        }

        public Department Register(Department department)
        {
            var addEvent = new OnRegisteredEvent()
            {
                Department = department
            };
            AddEvent(addEvent);
            return department;
        }

    }
}