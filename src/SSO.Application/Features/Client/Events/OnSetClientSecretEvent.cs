using OpenIddict.EntityFrameworkCore.Models;
using SSO.Domain.Base;

namespace SSO.Application.Features.Client.Events
{
    public class OnSetClientSecretEvent : BaseEvent
    {
        public OpenIddictEntityFrameworkCoreApplication Client { get; set; }
    }
}
