using MediatR;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SSO.Application.Features.Client.Events
{
    public class SettedClientSecretEventHandler : INotificationHandler<OnSetClientSecretEvent>
    {
        private readonly ILoggerFactory _logger;
        private readonly IOpenIddictApplicationManager _applicationManager;

        public SettedClientSecretEventHandler(
            ILoggerFactory logger
            , IOpenIddictApplicationManager applicationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
        }

        public async Task Handle(OnSetClientSecretEvent notification, CancellationToken cancellationToken)
        {
            var client = await _applicationManager.FindByIdAsync(notification?.Client.Id, cancellationToken) as OpenIddictEntityFrameworkCoreApplication;
            _logger.CreateLogger<SettedClientSecretEventHandler>()
                .LogTrace("Event with Id: {EventId} has been registered for client {client} ({Id})",
                    notification.EventId, client?.DisplayName, client?.Id);
        }
    }

}
