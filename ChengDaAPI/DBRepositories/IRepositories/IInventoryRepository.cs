
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;
using Domain.Models.Repository.Inventory;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IInventoryRepository : IDisposable
    {
        Task<List<Inventory>> GetAll();
        Task<dynamic> Insert(Inventory item);
        Task<Inventory> GetById(string id);
        void Remove(Inventory item);
        void Update(Inventory item);
        Task<List<Inventory>> GetList(GetListReq req);
        void AddRange(List<Inventory> items);
        void RemoveRange(List<Inventory> items);
        void UpdateRange(List<Inventory> items);
        Task<List<Inventory>> GetByStatus(string status);
    }
}
