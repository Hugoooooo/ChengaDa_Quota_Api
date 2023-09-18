
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;
using Domain.Models.Repository.ShipOrder;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IShipOrderRepository : IDisposable
    {
        Task<List<ShipOrder>> GetAll();
        Task<dynamic> Insert(ShipOrder item);
        Task<ShipOrder> GetById(string id);
        Task<List<ShipOrder>> GetList(GetListReq req);
        void Remove(ShipOrder item);
        void Update(ShipOrder item);
    }
}
