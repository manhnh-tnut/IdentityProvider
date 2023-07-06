
using AutoMapper;
using MediatR;
using SSO.Application.Features.Client.Responses;
using System.Runtime.Serialization;

namespace SSO.Application.Features.Client.Queries
{
    [DataContract]
    [AutoMap(typeof(GetClientQuery))]
    public class GetClientInfoQuery : IRequest<ClientInfoResponse>
    {
        [DataMember]
        public string Id { get; set; }
    }
}
