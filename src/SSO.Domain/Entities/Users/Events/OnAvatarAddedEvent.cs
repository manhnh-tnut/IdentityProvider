using SSO.Domain.Base;

namespace SSO.Domain.Entities.Users.Events
{
    public class OnAvatarAddedEvent : BaseEvent
    {
        public Avatar Avatar { get; set; }
    }
}