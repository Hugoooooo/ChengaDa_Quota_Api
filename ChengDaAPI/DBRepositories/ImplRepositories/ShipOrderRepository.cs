using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using Dapper;
using Domain.Models.Repository.ShipOrder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class ShipOrderRepository : IShipOrderRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public ShipOrderRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();


        public async Task<List<ShipOrder>> GetAll()
        {
            return await (from item in _coreDbContext.ShipOrder.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<ShipOrder> GetById(string id)
        {
            return await (from item in _coreDbContext.ShipOrder.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<List<ShipOrder>> GetList(GetListReq req)
        {
            #region Sql Condition
            string condition = @" WHERE 1=1 ";
            DynamicParameters dynamicParams = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(req.customer))
            {
                condition += $" AND customer like @customer";
                dynamicParams.Add("@customer", req.customer);
            }

            if (!string.IsNullOrWhiteSpace(req.id))
            {
                condition += $" AND id like @id";
                dynamicParams.Add("@id", $"%{req.id}%");
            }

            if (!string.IsNullOrWhiteSpace(req.sDate))
            {
                condition += $" AND ship_date >= @sDate";
                dynamicParams.Add("@sDate", req.sDate);
            }

            if (!string.IsNullOrWhiteSpace(req.eDate))
            {
                condition += $" AND ship_date <= @eDate";
                dynamicParams.Add("@eDate", req.eDate);
            }
            #endregion  

            string sql = $@"
                SELECT * FROM [ship_order] {condition}";

            return await Task.FromResult(_dapper.GetAll<ShipOrder>(sql, dynamicParams, commandType: CommandType.Text));
        }

        public async Task<dynamic> Insert(ShipOrder item)
        {
            await _coreDbContext.ShipOrder.AddAsync(item);
            return item;
        }

        public void Remove(ShipOrder item)
        {
            _coreDbContext.ShipOrder.Remove(item);
        }

        public void Update(ShipOrder item)
        {
            _coreDbContext.ShipOrder.Update(item);
        }

    }
}
