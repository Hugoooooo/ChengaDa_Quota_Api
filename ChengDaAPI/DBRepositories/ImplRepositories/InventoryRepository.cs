using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using Dapper;
using Domain.Models.Repository.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.ImplRepositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IDapperr _dapper;

        public InventoryRepository(CoreDbContext coreDbContext, IDapperr dapper)
        {
            _coreDbContext = coreDbContext;
            _dapper = dapper;
        }

        public void Dispose() => _coreDbContext.Dispose();


        public async Task<List<Inventory>> GetAll()
        {
            return await (from item in _coreDbContext.Inventory.AsNoTracking()
                          select item).ToListAsync();
        }

        public async Task<List<Inventory>> GetByStatus(string status)
        {
            return await (from item in _coreDbContext.Inventory.AsNoTracking()
                          where item.status == status
                          select item).ToListAsync();
        }

        public async Task<Inventory> GetById(string id)
        {
            return await (from item in _coreDbContext.Inventory.AsNoTracking()
                          where item.id == id
                          select item).FirstOrDefaultAsync();
        }

        public async Task<List<Inventory>> GetList(GetListReq req)
        {
            #region Sql Condition
            string condition = @" WHERE 1=1 ";
            DynamicParameters dynamicParams = new DynamicParameters();
            if (!string.IsNullOrEmpty(req.machineId))
            {
                condition += $" AND machineId like @machineId";
                dynamicParams.Add("@machineId", req.machineId);
            }

            if (!string.IsNullOrEmpty(req.brand))
            {
                condition += $" AND brand like @brand";
                dynamicParams.Add("@brand", req.brand);
            }

            if (!string.IsNullOrEmpty(req.pattern))
            {
                condition += $" AND pattern like @pattern";
                dynamicParams.Add("@pattern", req.pattern);
            }

            if (!string.IsNullOrEmpty(req.status))
            {
                condition += $" AND status = @status";
                dynamicParams.Add("@status", req.status);
            }

            if (req.machineIds.Count > 0)
            {
                condition += $" AND machineId IN @machineIds";
                dynamicParams.Add("@machineIds", req.machineIds);
            }

            if (req.ids.Count > 0)
            {
                condition += $" AND id IN @ids";
                dynamicParams.Add("@ids", req.ids);
            }
            #endregion  

            string sql = $@"
                SELECT * FROM [inventory] {condition}";

            return await Task.FromResult(_dapper.GetAll<Inventory>(sql, dynamicParams, commandType: CommandType.Text));
        }

        public async Task<dynamic> Insert(Inventory item)
        {
            await _coreDbContext.Inventory.AddAsync(item);
            return item;
        }

        public void Remove(Inventory item)
        {
            _coreDbContext.Inventory.Remove(item);
        }

        public void RemoveRange(List<Inventory> items)
        {
            _coreDbContext.Inventory.RemoveRange(items);
        }

        public void Update(Inventory item)
        {
            _coreDbContext.Inventory.Update(item);
        }

        public void AddRange(List<Inventory> items)
        {
            _coreDbContext.Inventory.AddRange(items);
        }

        public void UpdateRange(List<Inventory> items)
        {
            _coreDbContext.Inventory.UpdateRange(items);
        }
    }
}
