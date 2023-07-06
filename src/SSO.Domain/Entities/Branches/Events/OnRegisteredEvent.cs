using SSO.Domain.Base;

namespace SSO.Domain.Entities.Branches.Events
{
    public class OnRegisteredEvent : BaseEvent
    {
        public Branch Branch { get; set; }
    }
}