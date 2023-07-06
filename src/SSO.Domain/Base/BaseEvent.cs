using MediatR;

namespace SSO.Domain.Base
{
    public abstract class BaseEvent : INotification
    {
        public BaseEvent()
        {
            EventId = Guid.NewGuid();
            CreatedOn = DateTime.UtcNow;
        }

        public virtual Guid EventId { get; init; }
        public virtual DateTime CreatedOn { get; init; }
    }
}