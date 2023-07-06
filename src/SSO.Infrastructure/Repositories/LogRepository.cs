
using Microsoft.EntityFrameworkCore;
using SSO.Domain.Entities.Logs;
using SSO.Domain.Interfaces;
using SSO.Infrastructure.Contexts;
using System.Linq.Expressions;

namespace SSO.Infrastructure.Data.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly EFContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public LogRepository(EFContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Log> AddAsync(Log entity)
        {
            return (await _context.AddAsync(entity)).Entity;
        }

        public Task<bool> UpdateAsync(Log entity)
        {
            entity.Updated = DateTime.Now;
            return Task.FromResult(_context.Update(entity).Entity != null);
        }

        public Task<bool> DeleteAsync(Log entity, bool keep = true)
        {
            if (keep)
            {
                entity.Published = false;
                entity.Deleted = true;
                return UpdateAsync(entity);
            }
            return Task.FromResult(_context.Remove(entity).Entity != null);
        }

        public Task<Log> GetAsync(Expression<Func<Log, bool>> expression)
        {
            return _context.Set<Log>().FirstOrDefaultAsync(expression);
        }

        public Task<List<Log>> ListAsync(Expression<Func<Log, bool>> expression)
        {
            return _context.Set<Log>().Where(expression).ToListAsync();
        }
    }
}