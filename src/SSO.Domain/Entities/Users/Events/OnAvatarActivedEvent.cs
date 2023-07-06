using SSO.Domain.Base;

namespace SSO.Domain.Entities.Users.Events
{
    public class OnAvatarActivedEvent : BaseEvent
    {
        public Avatar Avatar { get; set; }
    }
}