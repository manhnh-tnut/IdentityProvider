using SSO.Domain.Base;

namespace SSO.Domain.Entities.Departments.Events
{
    public class OnRegisteredEvent : BaseEvent
    {
        public Department Department { get; set; }
    }
}