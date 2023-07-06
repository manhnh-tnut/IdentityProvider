using SSO.Domain.Entities.Branches;
using SSO.Infrastructure.Contexts;

namespace SSO.Infrastructure.Data.Repositories
{
    public class BranchRepository : EFRepository<Branch>
        , IBranchRepository
    {
        public BranchRepository(EFContext context) : base(context)
        {
        }
    }
}