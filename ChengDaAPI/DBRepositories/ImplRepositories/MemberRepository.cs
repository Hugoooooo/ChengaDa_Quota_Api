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
    public class MemberRepository : IMemberRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public MemberRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();

        public async Task<List<Member>> GetAll()
        {
            return await (from item in _coreDbContext.Member.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<Member> GetById(int memberId)
        {
            return await (from item in _coreDbContext.Member.AsNoTracking()
                          where item.id == memberId
                          select item).FirstOrDefaultAsync();
        }

        public async Task<dynamic> Insert(Member item)
        {
            await _coreDbContext.Member.AddAsync(item);
            return item;
        }

    }
}
