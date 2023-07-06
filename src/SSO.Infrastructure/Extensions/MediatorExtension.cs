using SSO.Domain.Base;
using MediatR;
using SSO.Infrastructure.Contexts;

namespace SSO.Infrastructure.Extensions;
static class MediatorExtension
{
    public static async Task DispatchEventsAsync(this IMediator mediator, EFContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Any());

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.Events)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}
