
using Microsoft.EntityFrameworkCore;
using SSO.Domain.Entities.Users;
using SSO.Domain.Interfaces;
using SSO.Infrastructure.Contexts;
using System.Linq.Expressions;

namespace SSO.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EFContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public UserRepository(EFContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> AddAsync(User entity)
        {
            return (await _context.AddAsync(entity)).Entity;
        }

        public Task<bool> UpdateAsync(User entity)
        {
            entity.Updated = DateTime.Now;
            return Task.FromResult(_context.Update(entity).Entity != null);
        }

        public Task<bool> DeleteAsync(User entity, bool keep = true)
        {
            if (keep)
            {
                entity.Published = false;
                entity.Deleted = true;
                return UpdateAsync(entity);
            }
            return Task.FromResult(_context.Remove(entity).Entity != null);
        }

        public Task<User> GetAsync(Expression<Func<User, bool>> expression)
        {
            return _context.Set<User>().FirstOrDefaultAsync(expression);
        }

        public Task<List<User>> ListAsync(Expression<Func<User, bool>> expression)
        {
            return _context.Set<User>().Where(expression).ToListAsync();
        }
    }
}