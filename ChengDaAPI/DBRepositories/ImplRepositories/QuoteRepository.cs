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
    public class QuoteRepository : IQuoteRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public QuoteRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();

        public async Task<List<Quote>> GetAll()
        {
            return await (from item in _coreDbContext.Quote.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<Quote> GetByCase(string caseNumber)
        {
            return await (from item in _coreDbContext.Quote.AsNoTracking()
                          where item.case_number == caseNumber
                          select item).FirstOrDefaultAsync();
        }

        public async Task<dynamic> Insert(Quote item)
        {
            await _coreDbContext.Quote.AddAsync(item);
            return item;
        }

    }
}
