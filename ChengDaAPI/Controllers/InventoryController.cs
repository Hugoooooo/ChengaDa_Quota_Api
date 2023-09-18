using AutoMapper;
using AutoMapper.Execution;
using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.ImplRepositories;
using ChengDaApi.DBRepositories.IRepositories;
using ChengDaApi.Helper;
using ChengDaApi.Services;
using DinkToPdf.Contracts;
using Domain;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Inventory;
using Domain.Models.Quote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChengDaApi.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly ILogger<InventoryController> _logger;
        private readonly JwtTokenService _jwtToken;
        private readonly IConfiguration _config;
        private readonly IDatabase _database;
        private readonly IAccountRepository _accountRepository;
        private readonly ISystemParameterRepository _systemParameterRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IPurchaseDetailRepository _purchaseDetailRepository;
        private readonly IShipOrderRepository _shipOrderRepository;
        private readonly IShipDetailRepository _shipDetailRepository;


        public InventoryController(ILogger<InventoryController> logger, IConfiguration configuration, IMapper mapper, IDatabase database, JwtTokenService jwtToken,
            IAccountRepository accountRepository, ISystemParameterRepository systemParameterRepository, IInventoryRepository inventoryRepository, IPurchaseDetailRepository purchaseDetailRepository,
            IPurchaseOrderRepository purchaseOrderRepository, IShipOrderRepository shipOrderRepository, IShipDetailRepository shipDetailRepository)
        {
            _logger = logger;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _database = database;
            _accountRepository = accountRepository;
            _systemParameterRepository = systemParameterRepository;
            _inventoryRepository = inventoryRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseDetailRepository = purchaseDetailRepository;
            _shipOrderRepository = shipOrderRepository;
            _shipDetailRepository = shipDetailRepository;
            _jwtToken = jwtToken;
        }

        #region 進貨單
        [HttpPost]
        [Route("addPurchaseOrder")]
        public async Task<AddPurchaseOrderRes> AddPurchaseOrder(AddPurchaseOrderReq req)
        {
            AddPurchaseOrderRes res = new AddPurchaseOrderRes()
            {
                message = GenericResType.INSERT_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                var addMachineIds = req.items.Select(p => p.machineId).ToList();
                var checkInventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { machineIds = addMachineIds });

                if (checkInventorys.Count > 0)
                {
                    res.isError = true;
                    res.message = "機號與庫存機器有重覆，請重新確認，重複型號如下:" + string.Join(',', checkInventorys.Select(p => p.machineId).ToList());
                    return res;
                }

                #endregion

                using (var transaction = _database.BeginTransaction())
                {
                    try
                    {
                        var inventorys = req.items.Select(p => new Inventory()
                        {
                            id = Utils.GenMixedUID(),
                            pattern = p.pattern,
                            machineId = p.machineId,
                            status = Inventory.STATUS_STOCK,
                            brand = p.brand,
                            price = p.price ?? 0,
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        }).ToList();

                        _inventoryRepository.AddRange(inventorys);

                        var orderId = Utils.GenMixedUID();
                        await _purchaseOrderRepository.Insert(new PurchaseOrder()
                        {
                            id = orderId,
                            type = req.type,
                            note = req.note,
                            purchase_date = DateTime.Parse(req.purchaseDate),
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        });

                        var orderDetails = inventorys.Select(p => new PurchaseDetail()
                        {
                            inventory_id = p.id,
                            order_id = orderId,
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        }).ToList();

                        _purchaseDetailRepository.AddRange(orderDetails);

                        await _database.SaveChangedAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Save To DB Exception：{ex}");
                        res.isError = true;
                        res.message = GenericResType.DB_ERROR;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("updatePurchaseOrder")]
        public async Task<AddPurchaseOrderRes> UpdatePurchaseOrder(UpdatePurchaseOrderReq req)
        {
            AddPurchaseOrderRes res = new AddPurchaseOrderRes()
            {
                message = GenericResType.UPDATE_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }
                var order = await _purchaseOrderRepository.GetById(req.orderId);
                var details = await _purchaseDetailRepository.GetByOrderId(req.orderId);
                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { ids = details.Select(p => p.inventory_id).ToList() });

                var addMachineIds = req.items.Select(p => p.machineId).ToList();
                var existInventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { machineIds = addMachineIds });
                var expectMachineIds = existInventorys.Select(p => p.machineId).ToList().Except(inventorys.Select(p => p.machineId).ToList());

                if (expectMachineIds.Count() > 0)
                {
                    res.isError = true;
                    res.message = "機號與庫存機器有重覆，請重新確認，重複型號如下:" + string.Join(',', expectMachineIds);
                    return res;
                }

                #endregion

                using (var transaction = _database.BeginTransaction())
                {
                    try
                    {
                        order.type = req.type;
                        order.purchase_date = DateTime.Parse(req.purchaseDate);
                        order.note = req.note;
                        order.update_date = now;
                        order.update_member = token.memberId;

                        _purchaseOrderRepository.Update(order);

                        var updateItems = req.items.Where(p => p.id > 0).ToList();

                        // 需要刪除的
                        var curDetailIds = updateItems.Select(p => p.id).ToList();
                        var delDetails = details.Where(p => !curDetailIds.Contains(p.id)).ToList();
                        var delInventoryIds = delDetails.Select(p => p.inventory_id).ToList();
                        var delInventorys = inventorys.Where(i => delInventoryIds.Contains(i.id)).ToList();

                        var disableInventorys = delInventorys.Where(p => p.status != Inventory.STATUS_STOCK).ToList();
                        if (disableInventorys.Count > 0)
                        {
                            res.isError = true;
                            res.message = "有異動包含非庫存商品，請重新確認，型號如下:" + string.Join(',', disableInventorys.Select(p => p.machineId).ToList());
                            return res;
                        }
                        _purchaseDetailRepository.RemoveRange(delDetails);
                        _inventoryRepository.RemoveRange(delInventorys);

                        // 需要新增的
                        var addItems = req.items.Where(p => p.id == 0 || p.id == null).ToList();
                        var addInventorys = addItems.Select(p => new Inventory()
                        {
                            id = Utils.GenMixedUID(),
                            pattern = p.pattern,
                            machineId = p.machineId,
                            status = Inventory.STATUS_STOCK,
                            brand = p.brand,
                            price = p.price ?? 0,
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        }).ToList();

                        _inventoryRepository.AddRange(addInventorys);

                        var orderDetails = addInventorys.Select(p => new PurchaseDetail()
                        {
                            inventory_id = p.id,
                            order_id = order.id,
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        }).ToList();

                        _purchaseDetailRepository.AddRange(orderDetails);

                        // 需要更新的

                        foreach (var updItem in updateItems)
                        {
                            var updDetail = details.First(p => p.id == updItem.id);
                            updDetail.update_member = token.memberId;
                            updDetail.update_date = now;

                            _purchaseDetailRepository.Update(updDetail);

                            var updInventory = inventorys.First(p => p.id == updItem.inventoryId);
                            updInventory.brand = updItem.brand;
                            updInventory.pattern = updItem.pattern;
                            updInventory.price = updItem.price ?? 0;
                            updInventory.update_date = now;
                            updInventory.update_member = token.memberId;

                            _inventoryRepository.Update(updInventory);
                        }


                        await _database.SaveChangedAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Save To DB Exception：{ex}");
                        res.isError = true;
                        res.message = GenericResType.DB_ERROR;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.UPDATE_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.UPDATE_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("removePurchaseOrder")]
        public async Task<GenericRes> RemovePurchaseOrder(RemovePurchaseOrderReq req)
        {
            GenericRes res = new GenericRes()
            {
                message = GenericResType.DELETE_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                var order = await _purchaseOrderRepository.GetById(req.orderId);
                var details = await _purchaseDetailRepository.GetByOrderId(req.orderId);
                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { ids = details.Select(p => p.inventory_id).ToList() });

                if (inventorys.Exists(p => p.status != Inventory.STATUS_STOCK))
                {
                    res.isError = true;
                    res.message = "此進貨單的商品有包含非庫存的資料，故無法刪除";
                    return res;
                }

                #endregion

                using (var transaction = _database.BeginTransaction())
                {
                    try
                    {

                        _purchaseOrderRepository.Remove(order);
                        _purchaseDetailRepository.RemoveRange(details);
                        _inventoryRepository.RemoveRange(inventorys);

                        await _database.SaveChangedAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Save To DB Exception：{ex}");
                        res.isError = true;
                        res.message = GenericResType.DB_ERROR;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.DELETE_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.DELETE_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("getPurchaseOrderList")]
        public async Task<GetPurchaseOrderListRes> GetPurchaseOrderList(string sDate, string eDate, string orderId)
        {
            GetPurchaseOrderListRes res = new GetPurchaseOrderListRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                #endregion
                var orders = await _purchaseOrderRepository.GetList(new Domain.Models.Repository.PurchaseOrder.GetListReq()
                {
                    sDate = sDate,
                    eDate = eDate,
                    id = orderId
                });

                if (orders.Count == 0)
                {
                    res.isError = true;
                    res.message = GenericResType.ITEM_NOT_FOUND;
                    return res;
                }

                var account = await _accountRepository.GetAll();

                var details = await _purchaseDetailRepository.GetList(new Domain.Models.Repository.PurchaseDetail.GetListReq()
                {
                    orderIds = orders.Select(p => p.id).ToList()
                });

                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq()
                {
                    ids = details.Select(p => p.inventory_id).ToList()
                });

                var purchaseOrderModels = orders.Select(order => new PurchaseOrderModel
                {
                    id = order.id,
                    note = order.note,
                    type = order.type,
                    createMember = account.FirstOrDefault(p => p.id == order.create_member)?.name,
                    purchaseDate = order.purchase_date,
                    detail = details
                        .Where(detail => detail.order_id == order.id)
                        .Select(detail =>
                        {
                            var inventory = inventorys.FirstOrDefault(inv => inv.id == detail.inventory_id);
                            return new PurchaseDetailModel
                            {
                                id = detail.id,
                                inventoryId = detail.inventory_id,
                                pattern = inventory?.pattern,
                                machineId = inventory?.machineId,
                                status = inventory?.status,
                                brand = inventory?.brand,
                                price = inventory?.price
                            };
                        }).ToList()
                }).ToList();

                res.items = purchaseOrderModels.OrderByDescending(p => p.purchaseDate).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("getPurchaseOrderById")]
        public async Task<GetPurchaseOrderByIdRes> GetPurchaseOrderById(string orderId)
        {
            GetPurchaseOrderByIdRes res = new GetPurchaseOrderByIdRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                #endregion
                var order = await _purchaseOrderRepository.GetById(orderId);

                if (order == null)
                {
                    res.isError = true;
                    res.message = GenericResType.ITEM_NOT_FOUND;
                    return res;
                }

                var account = await _accountRepository.GetAll();

                var details = await _purchaseDetailRepository.GetByOrderId(order.id);

                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq()
                {
                    ids = details.Select(p => p.inventory_id).ToList()
                });

                res.detail = new PurchaseOrderModel
                {
                    id = order.id,
                    note = order.note,
                    type = order.type,
                    createMember = account.FirstOrDefault(p => p.id == order.create_member)?.name,
                    purchaseDate = order.purchase_date,
                    detail = details
                        .Where(detail => detail.order_id == order.id)
                        .Select(detail =>
                        {
                            var inventory = inventorys.FirstOrDefault(inv => inv.id == detail.inventory_id);
                            return new PurchaseDetailModel
                            {
                                id = detail.id,
                                inventoryId = detail.inventory_id,
                                pattern = inventory?.pattern,
                                machineId = inventory?.machineId,
                                status = inventory?.status,
                                brand = inventory?.brand,
                                price = inventory?.price
                            };
                        }).ToList()
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }
        #endregion

        [HttpGet]
        [Route("getInventoryList")]
        public async Task<GetInventoryListRes> GetInventoryList(string pattern, string machineId, string status, string brand)
        {
            GetInventoryListRes res = new GetInventoryListRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                #endregion

                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq()
                {
                    pattern = $"%{pattern}%",
                    brand = $"%{brand}%",
                    machineId = $"%{machineId}%",
                    status = $"{status}",
                });
                var purchaseDetails = await _purchaseDetailRepository.GetAll();
                var shipDetails = await _shipDetailRepository.GetAll();

                foreach (var inv in inventorys)
                {
                    var pDetail = purchaseDetails.FirstOrDefault(p => p.inventory_id == inv.id);
                    var sDetail = shipDetails.FirstOrDefault(p => p.inventory_id == inv.id);
                    res.items.Add(new InventoryItem
                    {
                        id = inv.id,
                        brand = inv.brand,
                        status = inv.status,
                        machineId = inv.machineId,
                        price = inv.price,
                        pattern = inv.pattern,
                        purchaseDetailId = pDetail?.id ?? 0,
                        purchaseOrderId = pDetail?.order_id ?? string.Empty,
                        shipDetailId = sDetail?.id ?? 0,
                        shipOrderId = sDetail?.order_id ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("getInventoryStock")]
        public async Task<GetInventoryStockRes> GetInventoryStock(string shipOrderId)
        {
            GetInventoryStockRes res = new GetInventoryStockRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                #endregion

                var inventorys = await _inventoryRepository.GetAll();
                var shipDetails = await _shipDetailRepository.GetByOrderId(shipOrderId);
                var curInvenIds = shipDetails.Select(p => p.inventory_id).ToList();
                inventorys = inventorys.Where(p => p.status == Inventory.STATUS_STOCK || curInvenIds.Contains(p.id)).ToList();
                foreach (var inv in inventorys)
                {
                    var sDetail = shipDetails.FirstOrDefault(p => p.inventory_id == inv.id);
                    res.items.Add(new InventoryStockItem
                    {
                        id = inv.id,
                        brand = inv.brand,
                        machineId = inv.machineId,
                        price = inv.price,
                        pattern = inv.pattern,
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        #region 出貨單

        [HttpGet]
        [Route("getShipOrderList")]
        public async Task<GetShipOrderListRes> GetShipOrderList(string sDate, string eDate, string orderId)
        {
            GetShipOrderListRes res = new GetShipOrderListRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                #endregion
                var orders = await _shipOrderRepository.GetList(new Domain.Models.Repository.ShipOrder.GetListReq()
                {
                    sDate = sDate,
                    eDate = eDate,
                    id = orderId
                });

                if (orders.Count == 0)
                {
                    res.isError = true;
                    res.message = GenericResType.ITEM_NOT_FOUND;
                    return res;
                }

                var account = await _accountRepository.GetAll();

                var details = await _shipDetailRepository.GetList(new Domain.Models.Repository.ShipDetail.GetListReq()
                {
                    orderIds = orders.Select(p => p.id).ToList()
                });

                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq()
                {
                    ids = details.Select(p => p.inventory_id).ToList()
                });

                var shipOrderModels = orders.Select(order => new ShipOrderModel
                {
                    id = order.id,
                    type = order.type,
                    customer = order.customer,
                    amount = order.amount,
                    tax_type = order.tax_type,
                    invoice = order.invoice,
                    note = order.note,
                    createMember = account.FirstOrDefault(p => p.id == order.create_member)?.name,
                    shipDate = order.ship_date,
                    detail = details
                        .Where(detail => detail.order_id == order.id)
                        .Select(detail =>
                        {
                            var inventory = inventorys.FirstOrDefault(inv => inv.id == detail.inventory_id);
                            return new ShipDetailModel
                            {
                                id = detail.id,
                                inventoryId = detail.inventory_id,
                                pattern = inventory?.pattern,
                                machineId = inventory?.machineId,
                                status = inventory?.status,
                                brand = inventory?.brand,
                                price = inventory?.price
                            };
                        }).ToList()
                }).ToList();

                res.items = shipOrderModels.OrderByDescending(p => p.shipDate).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("addShipOrder")]
        public async Task<AddShipOrderRes> AddShipOrder(AddShipOrderReq req)
        {
            AddShipOrderRes res = new AddShipOrderRes()
            {
                message = GenericResType.INSERT_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { ids = req.inventoryIds });

                if (inventorys.Exists(p => p.status != Inventory.STATUS_STOCK))
                {
                    res.isError = true;
                    res.message = "出貨單選擇商品有包含非庫存的，請重新確認，重複型號如下:" + string.Join(',', inventorys.Where(p => p.status != Inventory.STATUS_STOCK).Select(p => p.machineId).ToList());
                    return res;
                }

                #endregion

                using (var transaction = _database.BeginTransaction())
                {
                    try
                    {
                        var updatedInventorys = inventorys.Select(inventory =>
                        {
                            inventory.status = Inventory.STATUS_SHIPPED;
                            return inventory;
                        }).ToList();

                        _inventoryRepository.UpdateRange(updatedInventorys);

                        var orderId = Utils.GenMixedUID();
                        await _shipOrderRepository.Insert(new ShipOrder()
                        {
                            id = orderId,
                            type = req.type,
                            customer = req.customer,
                            amount = req.amount ?? 0,
                            invoice = req.invoice,
                            tax_type = req.taxType,
                            note = req.note,
                            ship_date = DateTime.Parse(req.shipDate),
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        });

                        var orderDetails = req.inventoryIds.Select(p => new ShipDetail()
                        {
                            inventory_id = p,
                            order_id = orderId,
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        }).ToList();

                        _shipDetailRepository.AddRange(orderDetails);

                        await _database.SaveChangedAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Save To DB Exception：{ex}");
                        res.isError = true;
                        res.message = GenericResType.DB_ERROR;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("removeShipOrder")]
        public async Task<GenericRes> RemoveShipOrder(RemoveShipOrderReq req)
        {
            GenericRes res = new GenericRes()
            {
                message = GenericResType.DELETE_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                var order = await _shipOrderRepository.GetById(req.orderId);
                var details = await _shipDetailRepository.GetByOrderId(req.orderId);
                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { ids = details.Select(p => p.inventory_id).ToList() });

                #endregion

                using (var transaction = _database.BeginTransaction())
                {
                    try
                    {

                        var updatedInventorys = inventorys.Select(inventory =>
                        {
                            inventory.status = Inventory.STATUS_STOCK;
                            return inventory;
                        }).ToList();

                        _inventoryRepository.UpdateRange(updatedInventorys);
                        _shipOrderRepository.Remove(order);
                        _shipDetailRepository.RemoveRange(details);

                        await _database.SaveChangedAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Save To DB Exception：{ex}");
                        res.isError = true;
                        res.message = GenericResType.DB_ERROR;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.DELETE_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.DELETE_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("updateShipOrder")]
        public async Task<GenericRes> UpdateShipOrder(UpdateShipOrderReq req)
        {
            GenericRes res = new GenericRes()
            {
                message = GenericResType.UPDATE_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }
                var order = await _shipOrderRepository.GetById(req.orderId);
                var details = await _shipDetailRepository.GetByOrderId(req.orderId);
                var curInventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { ids = req.inventoryIds });
                var oriInventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq() { ids = details.Select(p => p.inventory_id).ToList() });

                #endregion

                using (var transaction = _database.BeginTransaction())
                {
                    try
                    {
                        order.customer = req.customer;
                        order.type = req.type;
                        order.amount = req.amount ?? 0;
                        order.tax_type = req.taxType;
                        order.invoice = req.invoice;
                        order.note = req.note;
                        order.ship_date = DateTime.Parse(req.shipDate);
                        order.create_date = now;
                        order.create_member = token.memberId;
                        order.update_date = now;
                        order.update_member = token.memberId;

                        _shipOrderRepository.Update(order);

                        // 找出需要更新回庫存的
                        var oriInventoryIds = details.Select(p => p.inventory_id).ToList();
                        var curInventoryIds = curInventorys.Select(p => p.id).ToList();
                        var delInventoryIds = oriInventoryIds.Except(curInventoryIds);

                        var delInventorys = oriInventorys.Where(p => delInventoryIds.Contains(p.id)).Select(inventory =>
                        {
                            inventory.status = Inventory.STATUS_STOCK;
                            return inventory;
                        }).ToList();

                        _inventoryRepository.UpdateRange(delInventorys);

                        // Inventory Detail 先清空再新增
                        _shipDetailRepository.RemoveRange(details);

                        var orderDetails = req.inventoryIds.Select(p => new ShipDetail()
                        {
                            inventory_id = p,
                            order_id = order.id,
                            create_member = token.memberId,
                            create_date = now,
                            update_member = token.memberId,
                            update_date = now
                        }).ToList();

                        _shipDetailRepository.AddRange(orderDetails);

                        var updatedInventorys = curInventorys.Select(inventory =>
                        {
                            inventory.status = Inventory.STATUS_SHIPPED;
                            return inventory;
                        }).ToList();

                        _inventoryRepository.UpdateRange(updatedInventorys);


                        await _database.SaveChangedAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Save To DB Exception：{ex}");
                        res.isError = true;
                        res.message = GenericResType.DB_ERROR;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.UPDATE_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.UPDATE_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("getShipOrderById")]
        public async Task<GetShipOrderByIdRes> getShipOrderById(string orderId)
        {
            GetShipOrderByIdRes res = new GetShipOrderByIdRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            var now = DateTime.Now;

            try
            {
                #region 檢核

                var token = _jwtToken.GetClaim(HttpContext);
                if (token == null)
                {
                    res.isError = true;
                    res.message = $"{GenericResType.PERMISSION_DENY}";
                    return res;
                }

                #endregion
                var order = await _shipOrderRepository.GetById(orderId);

                if (order == null)
                {
                    res.isError = true;
                    res.message = GenericResType.ITEM_NOT_FOUND;
                    return res;
                }

                var account = await _accountRepository.GetAll();

                var details = await _shipDetailRepository.GetByOrderId(order.id);

                var inventorys = await _inventoryRepository.GetList(new Domain.Models.Repository.Inventory.GetListReq()
                {
                    ids = details.Select(p => p.inventory_id).ToList()
                });

                res.detail = new ShipOrderModel
                {
                    id = order.id,
                    type = order.type,
                    customer = order.customer,
                    amount = order.amount,
                    tax_type = order.tax_type,
                    invoice = order.invoice,
                    note = order.note,
                    createMember = account.FirstOrDefault(p => p.id == order.create_member)?.name,
                    shipDate = order.ship_date,
                    detail = details
                        .Where(detail => detail.order_id == order.id)
                        .Select(detail =>
                        {
                            var inventory = inventorys.FirstOrDefault(inv => inv.id == detail.inventory_id);
                            return new ShipDetailModel
                            {
                                id = detail.id,
                                inventoryId = detail.inventory_id,
                                pattern = inventory?.pattern,
                                machineId = inventory?.machineId,
                                status = inventory?.status,
                                brand = inventory?.brand,
                                price = inventory?.price
                            };
                        }).ToList()
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }
        #endregion
    }
}
