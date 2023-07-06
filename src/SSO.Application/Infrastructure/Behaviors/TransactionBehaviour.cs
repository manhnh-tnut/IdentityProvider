
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using SSO.Application.Extensions;
using SSO.Infrastructure.Contexts;

namespace CWMS.Api.Infrastructure.Behaviors;
public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
    private readonly EFContext _context;

    public TransactionBehaviour(EFContext context,
        ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
    {
        _context = context ?? throw new ArgumentException(nameof(EFContext));
        _logger = logger ?? throw new ArgumentException(nameof(ILogger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        var typeName = request.GetGenericTypeName();

        try
        {
            if (_context.HasActiveTransaction)
            {
                return await next();
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                await using var transaction = await _context.BeginTransactionAsync();
                using (LogContext.PushProperty("TransactionContext", transaction.TransactionId))
                {
                    _logger.LogInformation("----- Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction.TransactionId, typeName, request);

                    response = await next();

                    _logger.LogInformation("----- Commit transaction {TransactionId} for {CommandName}", transaction.TransactionId, typeName);

                    await _context.CommitTransactionAsync(transaction);

                    transactionId = transaction.TransactionId;
                }
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Handling transaction for {CommandName} ({@Command})", typeName, request);
            throw;
        }
    }
}
