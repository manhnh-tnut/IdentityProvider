using SSO.Domain.Entities.Departments;
using SSO.Infrastructure.Contexts;

namespace SSO.Infrastructure.Data.Repositories
{
    public class DepartmentRepository : EFRepository<Department>
        , IDepartmentRepository
    {
        public DepartmentRepository(EFContext context) : base(context)
        {
        }
    }
}