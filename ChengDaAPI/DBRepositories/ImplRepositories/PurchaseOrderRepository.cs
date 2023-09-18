using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using Dapper;
using Domain.Models.Repository.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public PurchaseOrderRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();


        public async Task<List<PurchaseOrder>> GetAll()
        {
            return await (from item in _coreDbContext.PurchaseOrder.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<PurchaseOrder> GetById(string id)
        {
            return await (from item in _coreDbContext.PurchaseOrder.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<List<PurchaseOrder>> GetList(GetListReq req)
        {
            #region Sql Condition
            string condition = @" WHERE 1=1 ";
            DynamicParameters dynamicParams = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(req.id))
            {
                condition += $" AND id like @id";
                dynamicParams.Add("@id", $"%{req.id}%");
            }

            if (!string.IsNullOrWhiteSpace(req.sDate))
            {
                condition += $" AND purchase_date >= @sDate";
                dynamicParams.Add("@sDate", req.sDate);
            }

            if (!string.IsNullOrWhiteSpace(req.eDate))
            {
                condition += $" AND purchase_date <= @eDate";
                dynamicParams.Add("@eDate", req.eDate);
            }
            #endregion  

            string sql = $@"
                SELECT * FROM [purchase_order] {condition}";

            return await Task.FromResult(_dapper.GetAll<PurchaseOrder>(sql, dynamicParams, commandType: CommandType.Text));
        }

        public async Task<dynamic> Insert(PurchaseOrder item)
        {
            await _coreDbContext.PurchaseOrder.AddAsync(item);
            return item;
        }

        public void Remove(PurchaseOrder item)
        {
            _coreDbContext.PurchaseOrder.Remove(item);
        }

        public void Update(PurchaseOrder item)
        {
            _coreDbContext.PurchaseOrder.Update(item);
        }

    }
}
