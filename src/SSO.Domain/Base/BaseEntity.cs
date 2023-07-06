using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSO.Domain.Base
{
    public abstract class BaseEntity : IAggregateRoot
    {
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Updated { get; set; }
        public string UpdatedBy { get; set; }
        public bool Deleted { get; set; }
        public bool Published { get; set; }
        public BaseEntity()
        {
            Published = true;
            Deleted = false;
            Created = DateTime.Now;
            _events = new List<BaseEvent>();
        }

        [NotMapped]
        private readonly List<BaseEvent> _events;
        [NotMapped]
        public IReadOnlyList<BaseEvent> Events => _events.AsReadOnly();

        protected void AddEvent(BaseEvent @event)
        {
            _events?.Add(@event);
        }

        protected void RemoveEvent(BaseEvent @event)
        {
            _events?.Remove(@event);
        }

        public void ClearEvents()
        {
            _events?.Clear();
        }
    }

    public abstract class BaseEntity<TKey> : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TKey Id { get; set; }
    }
}