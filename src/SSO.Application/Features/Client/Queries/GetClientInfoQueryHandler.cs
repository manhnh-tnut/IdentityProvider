using AutoMapper;
using Dapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using OpenIddict.EntityFrameworkCore.Models;
using SSO.Application.Contracts;
using SSO.Application.Extensions;
using SSO.Application.Features.Client.Responses;
using SSO.Domain.Interfaces;
using System.Text;
using System.Text.Json;

namespace SSO.Application.Features.Client.Queries
{
    public class GetClientInfoQueryHandler : IRequestHandler<GetClientInfoQuery, ClientInfoResponse>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly IQueryRepository _queryRepository;

        // Using DI to inject infrastructure persistence Repositories
        public GetClientInfoQueryHandler(IMediator mediator
        , IDistributedCache cache
        , IQueryRepository queryRepository
        , IMapper mapper)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
        }
        /// <summary>
        /// Handler which processes the query when
        /// customer executes cancel user from app
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<ClientInfoResponse> Handle(GetClientInfoQuery query, CancellationToken cancellationToken)
        {
            return await _cache.GetOrSetCacheAsync(
                $"get-client-info: {JsonSerializer.Serialize(query, DistributedCacheContract.JsonOptions)}",
                async () =>
                {
                    StringBuilder sbQuery = new StringBuilder(1000);
                    sbQuery.Append(@"
                        SELECT [Id]
                            ,[ClientId]
                            ,[ClientSecret]
                            ,[ConcurrencyToken]
                            ,[ConsentType]
                            ,[DisplayName]
                            ,[DisplayNames]
                            ,[Permissions]
                            ,[PostLogoutRedirectUris]
                            ,[Properties]
                            ,[RedirectUris]
                            ,[Requirements]
                            ,[Type]
                        FROM [SSO].[dbo].[OpenIddictApplications]
                        WHERE [Id] = @Id
                    ");

                    using var conn = _queryRepository.Connection;
                    conn.Open();
                    var data = await conn.QueryFirstOrDefaultAsync<OpenIddictEntityFrameworkCoreApplication>(sbQuery.ToString(), new { query.Id });
                    return _mapper.Map<ClientInfoResponse>(data);
                }, DistributedCacheContract.Cache5M);
        }
    }
}
