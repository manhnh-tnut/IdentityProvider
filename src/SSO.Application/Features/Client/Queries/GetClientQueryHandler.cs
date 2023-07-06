using AutoMapper;
using Dapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using OpenIddict.EntityFrameworkCore.Models;
using SSO.Application.Contracts;
using SSO.Application.Extensions;
using SSO.Application.Features.Client.Responses;
using SSO.Domain.Interfaces;
using SSO.Infrastructure.Heplers;
using System.Data;
using System.Text;
using System.Text.Json;

namespace SSO.Application.Features.Client.Queries
{
    public class GetClientQueryHandler : IRequestHandler<GetClientQuery, IEnumerable<ClientResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly IQueryRepository _queryRepository;

        // Using DI to inject infrastructure persistence Repositories
        public GetClientQueryHandler(IMediator mediator
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
        public async Task<IEnumerable<ClientResponse>> Handle(GetClientQuery query, CancellationToken cancellationToken)
        {
            return await _cache.GetOrSetCacheAsync(
                $"get-clients: {JsonSerializer.Serialize(query, DistributedCacheContract.JsonOptions)}",
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
                        WHERE 1 = 1
                    ");

                    sbQuery.Append(QueryFilterBuilderHepler.FilterBuilder(query.Filters));
                    if (!string.IsNullOrEmpty(query.Keyword))
                    {
                        sbQuery.AppendLine(@"
                            AND (
                                [DisplayName] LIKE CONCAT('%',@Keyword,'%') 
                                OR [DisplayNames] LIKE CONCAT('%',@Keyword,'%') 
                                OR [RedirectUris] LIKE CONCAT('%',@Keyword,'%') 
                            )
                        ");
                    }

                    if (query.OrderBy != null && query.OrderBy.Any())
                    {
                        sbQuery.AppendLine($"ORDER BY {string.Join(", ", query.OrderBy)}");
                        if (query.Offset > 0)
                        {
                            sbQuery.AppendLine($"OFFSET {query.Offset} ROWS");
                        }
                        if (query.Limit > 0)
                        {
                            sbQuery.Append($"FETCH NEXT {query.Limit} ROWS ONLY");
                        }
                    }
                    using var conn = _queryRepository.Connection;
                    conn.Open();
                    var data = await conn.QueryAsync<OpenIddictEntityFrameworkCoreApplication>(sbQuery.ToString(), new { query.Keyword });
                    return data.Select(i => _mapper.Map<ClientResponse>(i));
                }, DistributedCacheContract.Cache5M);
        }
    }
}
