using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using Dapper;
using Domain.Models.Repository.PurchaseDetail;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class PurchaseDetailRepository : IPurchaseDetailRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public PurchaseDetailRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();


        public async Task<List<PurchaseDetail>> GetAll()
        {
            return await (from item in _coreDbContext.PurchaseDetail.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<PurchaseDetail> GetById(int id)
        {
            return await (from item in _coreDbContext.PurchaseDetail.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<List<PurchaseDetail>> GetByOrderId(string orderId)
        {
            return await (from item in _coreDbContext.PurchaseDetail.AsNoTracking()
                          where item.order_id == orderId
                          select item).ToListAsync();
        }

        public async Task<List<PurchaseDetail>> GetList(GetListReq req)
        {
            #region Sql Condition
            string condition = @" WHERE 1=1 ";
            DynamicParameters dynamicParams = new DynamicParameters();

            if (req.orderIds.Count > 0)
            {
                condition += $" AND order_id IN @orderIds";
                dynamicParams.Add("@orderIds", req.orderIds);
            }

            if (req.inventory_ids.Count > 0)
            {
                condition += $" AND inventory_id IN @inventory_ids";
                dynamicParams.Add("@inventory_ids", req.inventory_ids);
            }

            #endregion  

            string sql = $@"
                SELECT * FROM [purchase_detail] {condition}";

            return await Task.FromResult(_dapper.GetAll<PurchaseDetail>(sql, dynamicParams, commandType: CommandType.Text));
        }

        public async Task<dynamic> Insert(PurchaseDetail item)
        {
            await _coreDbContext.PurchaseDetail.AddAsync(item);
            return item;
        }

        public void Remove(PurchaseDetail item)
        {
            _coreDbContext.PurchaseDetail.Remove(item);
        }

        public void RemoveRange(List<PurchaseDetail> items)
        {
            _coreDbContext.PurchaseDetail.RemoveRange(items);
        }

        public void Update(PurchaseDetail item)
        {
            _coreDbContext.PurchaseDetail.Update(item);
        }

        public void AddRange(List<PurchaseDetail> items)
        {
            _coreDbContext.PurchaseDetail.AddRange(items);
        }

    }
}
