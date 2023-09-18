
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;
using Domain.Models.Repository.ShipDetail;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IShipDetailRepository : IDisposable
    {
        Task<List<ShipDetail>> GetAll();
        Task<dynamic> Insert(ShipDetail item);
        Task<ShipDetail> GetById(int id);
        Task<List<ShipDetail>> GetByOrderId(string orderId);
        Task<List<ShipDetail>> GetList(GetListReq req);
        void Remove(ShipDetail item);
        void Update(ShipDetail item);
        void AddRange(List<ShipDetail> items);
        void RemoveRange(List<ShipDetail> items);
    }
}
