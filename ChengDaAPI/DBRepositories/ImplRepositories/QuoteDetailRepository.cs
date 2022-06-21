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
    public class QuoteDetailRepository : IQuoteDetailRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public QuoteDetailRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();

        public async Task<List<QuoteDetail>> GetAll()
        {
            return await (from item in _coreDbContext.QuoteDetail.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<List<QuoteDetail>> GetByQId(int qid)
        {
            return await (from item in _coreDbContext.QuoteDetail.AsNoTracking()
                          where item.qid == qid
                          select item).ToListAsync();
        }

        public async Task<dynamic> Insert(QuoteDetail item)
        {
            await _coreDbContext.QuoteDetail.AddAsync(item);
            return item;
        }

    }
}
