using SSO.Domain.Base;
using System.Linq.Expressions;

namespace SSO.Domain.Interfaces
{
    public interface IEFRepository<T> where T : class, IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
        Task<T> AddAsync(T entity);

        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteAsync(T entity, bool keep = true);

        Task<T> GetAsync(Expression<Func<T, bool>> expression);

        Task<List<T>> ListAsync(Expression<Func<T, bool>> expression);
    }
}