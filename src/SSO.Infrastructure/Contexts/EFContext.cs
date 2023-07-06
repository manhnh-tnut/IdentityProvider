using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSO.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using SSO.Infrastructure.Data.EntitiesConfig;
using SSO.Infrastructure.Extensions;
using System.Data;
using SSO.Domain.Entities.Users;
using SSO.Domain.Entities.Roles;

namespace SSO.Infrastructure.Contexts;
public class EFContext : IdentityDbContext<User, Role, string>, IUnitOfWork
{
    public const string DEFAULT_SCHEMA = "dbo";
    private readonly IMediator _mediator;
    private IDbContextTransaction _currentTransaction;

    public EFContext(DbContextOptions<EFContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SSOUser Id=sa;Password=Abc!3579;TrustServerCertificate=True;Integrated Security=false;MultipleActiveResultSets=true;");
        }
    }

    public EFContext(DbContextOptions<EFContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        System.Diagnostics.Debug.WriteLine("EFContext::ctor ->" + this.GetHashCode());
    }

    public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //foreach (var entityType in builder.Model.GetEntityTypes())
        //{
        //    var tableName = entityType.GetTableName();
        //    if (tableName.StartsWith("AspNet"))
        //    {
        //        entityType.SetTableName(tableName[6..]);
        //        continue;
        //    }
        //    if (tableName.StartsWith("OpenIddict"))
        //    {
        //        entityType.SetTableName(tableName[10..]);
        //    }

        //}

        builder.ApplyConfiguration(new BranchEntityTypeConfig());
        builder.ApplyConfiguration(new DepartmentEntityTypeConfig());

        builder.ApplyConfiguration(new UserEntityTypeConfig());
        builder.ApplyConfiguration(new RoleEntityTypeConfig());

        builder.ApplyConfiguration(new LogEntityTypeConfig());
        builder.ApplyConfiguration(new AuditLogEntityTypeConfig());
    }

    public async Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch Domain Events collection. 
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
        await _mediator.DispatchEventsAsync(this);

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}