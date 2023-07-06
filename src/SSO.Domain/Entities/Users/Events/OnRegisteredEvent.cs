using SSO.Domain.Base;
using SSO.Domain.Entities.Users;

namespace SSO.Domain.Entities.Users.Events
{
    public class OnRegisteredEvent : BaseEvent
    {
        public User User { get; set; }
    }
}