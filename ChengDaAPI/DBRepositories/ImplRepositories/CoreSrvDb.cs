using System;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.IRepositories;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class CoreSrvDb : IDatabase
    {
        private readonly CoreDbContext _coreDbContext;

        public CoreSrvDb(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public dynamic BeginTransaction()
        {
            return _coreDbContext.Database.BeginTransaction();
        }

        public Task SaveChangedAsync()
        {
            return _coreDbContext.SaveChangesAsync();
        }
    }
}
