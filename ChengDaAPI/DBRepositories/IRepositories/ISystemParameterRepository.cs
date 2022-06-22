
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface ISystemParameterRepository : IDisposable
    {
        Task<List<SystemParameter>> GetAll();
    }
}
