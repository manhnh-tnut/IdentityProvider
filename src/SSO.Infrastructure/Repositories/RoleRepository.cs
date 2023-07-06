
using Microsoft.EntityFrameworkCore;
using SSO.Domain.Entities.Roles;
using SSO.Domain.Interfaces;
using SSO.Infrastructure.Contexts;
using System.Linq.Expressions;

namespace SSO.Infrastructure.Data.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly EFContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public RoleRepository(EFContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Role> AddAsync(Role entity)
        {
            return (await _context.AddAsync(entity)).Entity;
        }

        public Task<bool> UpdateAsync(Role entity)
        {
            entity.Updated = DateTime.Now;
            return Task.FromResult(_context.Update(entity).Entity != null);
        }

        public Task<bool> DeleteAsync(Role entity, bool keep = true)
        {
            if (keep)
            {
                entity.Published = false;
                entity.Deleted = true;
                return UpdateAsync(entity);
            }
            return Task.FromResult(_context.Remove(entity).Entity != null);
        }

        public Task<Role> GetAsync(Expression<Func<Role, bool>> expression)
        {
            return _context.Set<Role>().FirstOrDefaultAsync(expression);
        }

        public Task<List<Role>> ListAsync(Expression<Func<Role, bool>> expression)
        {
            return _context.Set<Role>().Where(expression).ToListAsync();
        }
    }
}