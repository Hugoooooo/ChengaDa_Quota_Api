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
    public class DayoffDetailRepository : IDayoffDetailRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public DayoffDetailRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();

        public async Task<List<DayoffDetail>> GetAll()
        {
            return await (from item in _coreDbContext.DayoffDetail.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<List<DayoffDetail>> GetDetailByMonth(DateTime startDate, DateTime endDate)
        {
            return await (from item in _coreDbContext.DayoffDetail.AsNoTracking()
                          where item.start_date >= startDate && item.end_date <= endDate
                          select item).ToListAsync();
        }

        public async Task<DayoffDetail> GetById(int id)
        {
            return await (from item in _coreDbContext.DayoffDetail.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<List<DayoffDetail>> GetByType(string type)
        {
            return await (from item in _coreDbContext.DayoffDetail.AsNoTracking()
                          where item.category == type
                          select item).ToListAsync();
        }

        public async Task<List<DayoffDetail>> GetDetailByMemberId(int memberId, DateTime startDate, DateTime endDate)
        {
            return await (from item in _coreDbContext.DayoffDetail.AsNoTracking()
                          where item.memberId == memberId && item.dayoff_date >= startDate && item.dayoff_date <= endDate
                          select item).ToListAsync();
        }

        public async Task<dynamic> Insert(DayoffDetail item)
        {
            await _coreDbContext.DayoffDetail.AddAsync(item);
            return item;
        }

        public void Remove(DayoffDetail item)
        {
            _coreDbContext.DayoffDetail.Remove(item);
        }

    }
}
