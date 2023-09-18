
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChengDaApi.DBRepositories.DBSchema;
using Domain.Models.Repository.PurchaseDetail;

namespace ChengDaApi.DBRepositories.IRepositories
{
    public interface IPurchaseDetailRepository : IDisposable
    {
        Task<List<PurchaseDetail>> GetAll();
        Task<dynamic> Insert(PurchaseDetail item);
        Task<PurchaseDetail> GetById(int id);
        Task<List<PurchaseDetail>> GetList(GetListReq req);
        Task<List<PurchaseDetail>> GetByOrderId(string orderId);
        void Remove(PurchaseDetail item);
        void Update(PurchaseDetail item);
        void AddRange(List<PurchaseDetail> items);
        void RemoveRange(List<PurchaseDetail> items);
    }
}
