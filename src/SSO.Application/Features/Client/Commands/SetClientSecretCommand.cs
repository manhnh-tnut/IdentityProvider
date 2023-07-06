using AutoMapper;
using MediatR;
using SSO.Application.Features.Client.Requests;
using System.Runtime.Serialization;

namespace SSO.Application.Features.Client.Commands
{
    [DataContract]
    [AutoMap(typeof(SetClientSecretRequest))]
    public class SetClientSecretCommand : IRequest<bool>
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Secret { get; set; }
    }
}
