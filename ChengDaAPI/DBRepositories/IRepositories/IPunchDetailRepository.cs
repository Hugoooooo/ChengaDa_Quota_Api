
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IPunchDetailRepository : IDisposable
    {
        Task<List<PunchDetail>> GetAll();
        Task<dynamic> Insert(PunchDetail item);
        Task<dynamic> AddRange(List<PunchDetail> items);
        Task<List<PunchDetail>> GetMonthDetailByMemberId(int memberId, DateTime startDate, DateTime endDate);
        Task<List<PunchDetail>> GetMonthDetailByMonth(DateTime startDate, DateTime endDate);
        void RemoveRange(List<PunchDetail> items);
    }
}
