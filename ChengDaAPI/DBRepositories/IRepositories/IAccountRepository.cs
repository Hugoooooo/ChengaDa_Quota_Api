
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IAccountRepository : IDisposable
    {
        Task<List<Account>> GetAll();
        Task<dynamic> Insert(Account item);
        Task<Account> GetById(string AccountId);
        void Remove(Account item);
        Task<Account> Login(string account, string pwd);
        Task<Account> GetByAccount(string account);
        void Update(Account item);
        Task<Account> GetByRefreshToken(string token);
    }
}
