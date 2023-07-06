using System.Reflection;
using Autofac;
using MediatR;
using SSO.Application.Features.Client.Queries;
using SSO.Domain.Interfaces;
using SSO.Infrastructure.Data.Repositories;

namespace SSO.Application.Infrastructure.AutofacModules;

public class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<QueryRepository>()
            .As<IQueryRepository>()
            .InstancePerLifetimeScope();
        builder.RegisterAssemblyTypes(
                typeof(GetClientQueryHandler).GetTypeInfo().Assembly,
                typeof(GetClientInfoQueryHandler).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<>));
    }
}
