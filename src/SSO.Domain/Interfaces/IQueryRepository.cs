using System.Data;

namespace SSO.Domain.Interfaces;

public interface IQueryRepository{
    IDbConnection Connection{ get; }
}