using SSO.Domain.Base;

namespace SSO.Domain.Entities.Roles.Events
{
    public class OnRegisteredEvent : BaseEvent
    {
        public Role Role { get; set; }
    }
}