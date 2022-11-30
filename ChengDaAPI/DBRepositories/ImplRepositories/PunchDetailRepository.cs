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
    public class PunchDetailRepository : IPunchDetailRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public PunchDetailRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();

        public async Task<List<PunchDetail>> GetAll()
        {
            return await (from item in _coreDbContext.PunchDetail.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<List<PunchDetail>> GetMonthDetailByMemberId(int memberId,DateTime startDate,DateTime endDate)
        {
            return await (from item in _coreDbContext.PunchDetail.AsNoTracking()
                          where item.member_id == memberId && item.punch_date >= startDate && item.punch_date <= endDate
                          select item).ToListAsync();
        }

        public async Task<List<PunchDetail>> GetMonthDetailByMonth(DateTime startDate, DateTime endDate)
        {
            return await (from item in _coreDbContext.PunchDetail.AsNoTracking()
                          where item.punch_date >= startDate && item.punch_date <= endDate
                          select item).ToListAsync();
        }
        public async Task<dynamic> Insert(PunchDetail item)
        {
            await _coreDbContext.PunchDetail.AddAsync(item);
            return item;
        }

        public async Task<dynamic> AddRange(List<PunchDetail> items)
        {
            await _coreDbContext.PunchDetail.AddRangeAsync(items);
            return items;
        }

        public void RemoveRange(List<PunchDetail> items)
        {
            _coreDbContext.PunchDetail.RemoveRange(items);
        }
    }
}
