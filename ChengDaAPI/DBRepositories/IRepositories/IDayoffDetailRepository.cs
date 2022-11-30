
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IDayoffDetailRepository : IDisposable
    {
        Task<List<DayoffDetail>> GetAll();
        Task<dynamic> Insert(DayoffDetail item);

        void Remove(DayoffDetail item);
        Task<DayoffDetail> GetById(int id);
        Task<List<DayoffDetail>> GetByType(string type);
        Task<List<DayoffDetail>> GetDetailByMemberId(int memberId, DateTime startDate, DateTime endDate);
    }
}
