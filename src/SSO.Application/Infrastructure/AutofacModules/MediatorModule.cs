using System.Reflection;
using Autofac;
using CWMS.Api.Infrastructure.Behaviors;
using FluentValidation;
using MediatR;
using SSO.Application.Features.Client.Commands;
using SSO.Application.Features.Client.Validations;
using SSO.Application.Infrastructure.Behaviors;

namespace SSO.Application.Infrastructure.AutofacModules;

public class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
            .AsImplementedInterfaces();

        // Register all the Command and Query classes (they implement IRequestHandler) in assembly holding the Commands
        builder.RegisterAssemblyTypes(
                //typeof(SetClientSecretCommand).GetTypeInfo().Assembly,
                typeof(SetClientSecretCommand).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>));

        // // Register the DomainEventHandler classes (they implement INotificationHandler<>) in assembly holding the Domain Events
        //builder.RegisterAssemblyTypes(typeof(RegisteredEventHandler).GetTypeInfo().Assembly)
        //    .AsClosedTypesOf(typeof(INotificationHandler<>));

        // Register the Command's Validators (Validators based on FluentValidation library)
        builder
            .RegisterAssemblyTypes(typeof(SetClientSecretCommandValidator).GetTypeInfo().Assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(LoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(TransactionBehaviour<,>)).As(typeof(IPipelineBehavior<,>));
    }
}
