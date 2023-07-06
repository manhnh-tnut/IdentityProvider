
using Microsoft.EntityFrameworkCore;
using SSO.Domain.Entities.AuditLogs;
using SSO.Domain.Interfaces;
using SSO.Infrastructure.Contexts;
using System.Linq.Expressions;

namespace SSO.Infrastructure.Data.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly EFContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public AuditLogRepository(EFContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AuditLog> AddAsync(AuditLog entity)
        {
            return (await _context.AddAsync(entity)).Entity;
        }

        public Task<bool> UpdateAsync(AuditLog entity)
        {
            entity.Updated = DateTime.Now;
            return Task.FromResult(_context.Update(entity).Entity != null);
        }

        public Task<bool> DeleteAsync(AuditLog entity, bool keep = true)
        {
            if (keep)
            {
                entity.Published = false;
                entity.Deleted = true;
                return UpdateAsync(entity);
            }
            return Task.FromResult(_context.Remove(entity).Entity != null);
        }

        public Task<AuditLog> GetAsync(Expression<Func<AuditLog, bool>> expression)
        {
            return _context.Set<AuditLog>().FirstOrDefaultAsync(expression);
        }

        public Task<List<AuditLog>> ListAsync(Expression<Func<AuditLog, bool>> expression)
        {
            return _context.Set<AuditLog>().Where(expression).ToListAsync();
        }
    }
}