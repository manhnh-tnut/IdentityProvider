using SSO.Domain.Base;

namespace SSO.Domain.Entities.Users
{
    public class Avatar : BaseEntity<Guid>
    {
        public Avatar(
            string userId
            ,string name
        ) : base()
        {
            Name = name;
            UserId = userId;
        }
        public string UserId { get; private set; }
        public string Name { get; internal set; }
        public bool IsActived { get; internal set; }
        public virtual User User { get; private set; }
    }
}