
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IQuoteDetailRepository : IDisposable
    {
        Task<List<QuoteDetail>> GetAll();
        Task<List<QuoteDetail>> GetByQId(int qid);
        Task<dynamic> Insert(QuoteDetail item);
    }
}
