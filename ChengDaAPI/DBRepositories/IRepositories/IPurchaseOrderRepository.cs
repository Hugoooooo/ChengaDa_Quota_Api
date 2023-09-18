
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;
using Domain.Models.Repository.PurchaseOrder;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IPurchaseOrderRepository : IDisposable
    {
        Task<List<PurchaseOrder>> GetAll();
        Task<dynamic> Insert(PurchaseOrder item);
        Task<PurchaseOrder> GetById(string id);
        Task<List<PurchaseOrder>> GetList(GetListReq req);
        void Remove(PurchaseOrder item);
        void Update(PurchaseOrder item);
    }
}
