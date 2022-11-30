
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IMemberRepository : IDisposable
    {
        Task<List<Member>> GetAll();
        Task<dynamic> Insert(Member item);
        Task<Member> GetById(int memberId);
    }
}
