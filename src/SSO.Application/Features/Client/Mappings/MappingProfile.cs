using AutoMapper;
using OpenIddict.EntityFrameworkCore.Models;
using SSO.Application.Features.Client.Commands;
using SSO.Application.Features.Client.Queries;
using SSO.Application.Features.Client.Requests;
using SSO.Application.Features.Client.Responses;

namespace SSO.Application.Features.Client.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GetClientRequest, GetClientQuery>();
            CreateMap<OpenIddictEntityFrameworkCoreApplication, ClientResponse>();

            CreateMap<GetClientInfoRequest, GetClientInfoQuery>();
            CreateMap<OpenIddictEntityFrameworkCoreApplication, ClientInfoResponse>();

            CreateMap<SetClientSecretRequest, SetClientSecretCommand>();
        }
    }
}
