using AutoMapper;
using Azure.Core;
using MediatR;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using SSO.Application.Features.Client.Queries;
using SSO.Application.Features.Client.Requests;

namespace SSO.Application.Features.Client.Commands
{
    public class SetClientSecretCommandHandler : IRequestHandler<SetClientSecretCommand, bool>
    {
        private readonly ILogger<SetClientSecretCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IOpenIddictApplicationManager _applicationManager;

        // Using DI to inject infrastructure persistence Repositories
        public SetClientSecretCommandHandler(
            IMapper mapper
            , IMediator mediator
            , IOpenIddictApplicationManager applicationManager
            , ILogger<SetClientSecretCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
        }
        /// <summary>
        /// Handler which processes the command when
        /// customer executes cancel client from app
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(SetClientSecretCommand command, CancellationToken cancellationToken)
        {
            var client = await _applicationManager.FindByIdAsync(command?.Id, cancellationToken) as OpenIddictEntityFrameworkCoreApplication ?? throw new Exception(string.Format("Client {0} not found!", command?.Id));
            _logger.LogInformation("----- Updating client: {@client}", client.ClientId);
            client.ClientSecret = "New-Secret";
            await _applicationManager.UpdateAsync(client, cancellationToken);
            //var @event = new OnRegisteredEvent()
            //{
            //    Compound = compound
            //};
            //AddEvent(@event);
            return true;
        }
    }
}
