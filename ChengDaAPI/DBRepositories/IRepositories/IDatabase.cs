using System;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IDatabase
    {
        dynamic BeginTransaction();
        Task SaveChangedAsync();
    }
}
