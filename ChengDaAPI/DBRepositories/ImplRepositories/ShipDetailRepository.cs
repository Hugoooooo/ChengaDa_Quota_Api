using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using Dapper;
using Domain.Models.Repository.ShipDetail;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class ShipDetailRepository : IShipDetailRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public ShipDetailRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();


        public async Task<List<ShipDetail>> GetAll()
        {
            return await (from item in _coreDbContext.ShipDetail.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<ShipDetail> GetById(int id)
        {
            return await (from item in _coreDbContext.ShipDetail.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<List<ShipDetail>> GetByOrderId(string orderId)
        {
            return await (from item in _coreDbContext.ShipDetail.AsNoTracking()
                          where item.order_id == orderId
                          select item).ToListAsync();
        }

        public async Task<List<ShipDetail>> GetList(GetListReq req)
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
                SELECT * FROM [ship_detail] {condition}";

            return await Task.FromResult(_dapper.GetAll<ShipDetail>(sql, dynamicParams, commandType: CommandType.Text));
        }

        public async Task<dynamic> Insert(ShipDetail item)
        {
            await _coreDbContext.ShipDetail.AddAsync(item);
            return item;
        }

        public void Remove(ShipDetail item)
        {
            _coreDbContext.ShipDetail.Remove(item);
        }

        public void Update(ShipDetail item)
        {
            _coreDbContext.ShipDetail.Update(item);
        }

        public void AddRange(List<ShipDetail> items)
        {
            _coreDbContext.ShipDetail.AddRange(items);
        }

        public void RemoveRange(List<ShipDetail> items)
        {
            _coreDbContext.ShipDetail.RemoveRange(items);
        }
    }
}
