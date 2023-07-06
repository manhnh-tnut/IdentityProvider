
using AutoMapper;
using MediatR;
using SSO.Application.Features.Client.Responses;
using SSO.Application.Features.Common.Queries;
using System.Runtime.Serialization;

namespace SSO.Application.Features.Client.Queries
{
    [DataContract]
    [AutoMap(typeof(GetClientQuery))]
    public class GetClientQuery : BaseQuery, IRequest<IEnumerable<ClientResponse>>
    {
    }
}
