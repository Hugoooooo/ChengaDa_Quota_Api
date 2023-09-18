using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public AccountRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();

        public async Task<List<Account>> GetAll()
        {
            return await (from item in _coreDbContext.Account.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<Account> GetById(string id)
        {
            return await (from item in _coreDbContext.Account.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<Account> GetByAccount(string account)
        {
            return await (from item in _coreDbContext.Account.AsNoTracking()
                          where item.account == account
                          select item).FirstOrDefaultAsync();
        }

        public async Task<Account> Login(string account, string pwd)
        {
            return await (from item in _coreDbContext.Account.AsNoTracking()
                          where item.account == account && item.pwd == pwd
                          select item).FirstOrDefaultAsync();
        }

        public async Task<dynamic> Insert(Account item)
        {
            await _coreDbContext.Account.AddAsync(item);
            return item;
        }

        public void Remove(Account item)
        {
            _coreDbContext.Account.Remove(item);
        }

        public void Update(Account item)
        {
            _coreDbContext.Account.Update(item);
        }

        public async Task<Account> GetByRefreshToken(string token)
        {
            return await (from item in _coreDbContext.Account.AsNoTracking()
                          where item.refreshToken == token
                          select item).FirstOrDefaultAsync();
        }

    }
}
