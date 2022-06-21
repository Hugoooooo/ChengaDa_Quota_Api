
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IQuoteRepository : IDisposable
    {
        Task<List<Quote>> GetAll();
        Task<Quote> GetByCase(string caseNumber);
        Task<dynamic> Insert(Quote item);
    }
}
