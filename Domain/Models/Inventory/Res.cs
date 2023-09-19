using System;
using System.Collections.Generic;

namespace Domain.Models.Inventory
{
    public class AddPurchaseOrderRes : GenericRes
    {

    }

    public class AddShipOrderRes : GenericRes
    {

    }

    public class GetPurchaseOrderByIdRes : GenericRes
    {
        public PurchaseOrderModel detail { get; set; }
    }

    public class GetPurchaseOrderListRes : GenericRes
    {
        public List<PurchaseOrderModel> items { get; set; } = new List<PurchaseOrderModel>();
    }

    public class PurchaseOrderModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string note { get; set; }
        public string createMember { get; set; }
        public DateTime purchaseDate { get; set; }
        public List<PurchaseDetailModel> detail { get; set; } = new List<PurchaseDetailModel>();
    }
    
    public class GetShipOrderByIdRes : GenericRes
    {
        public ShipOrderModel detail { get; set; }
    }

    public class GetShipOrderListRes : GenericRes
    {
        public List<ShipOrderModel> items { get; set; } = new List<ShipOrderModel>();
    }

    public class ShipOrderModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string customer { get; set; }
        public int amount { get; set; }
        public string tax_type { get; set; }
        public string invoice { get; set; }
        public string note { get; set; }
        public DateTime shipDate { get; set; }
        public string createMember { get; set; }
        public List<ShipDetailModel> detail { get; set; } = new List<ShipDetailModel>();
    }

    public class ShipDetailModel: PurchaseDetailModel
    {

    }
    public class PurchaseDetailModel
    {
        public int id { get; set; }
        public string inventoryId { get; set; }
        public string pattern { get; set; }
        public string machineId { get; set; }
        public string status { get; set; }
        public string brand { get; set; }
        public int? price { get; set; }

    }

    public class GetInventoryListRes : GenericRes
    {
        public List<InventoryItem> items { get; set; } = new List<InventoryItem>();

    }

    public class InventoryItem
    {
        public string id { get; set; }
        public string pattern { get; set; }
        public string machineId { get; set; }
        public string status { get; set; }
        public string brand { get; set; }
        public int price { get; set; }
        public int purchaseDetailId { get; set; }
        public string purchaseOrderId { get; set; }
        public int shipDetailId { get; set; }
        public string shipOrderId { get; set; }
    }

    public class GetInventoryStockRes: GenericRes
    {
        public List<InventoryStockItem> items { get; set; } = new List<InventoryStockItem>();
    }

    public class InventoryStockItem
    {
        public string id { get; set; }
        public string pattern { get; set; }
        public string machineId { get; set; }
        public string brand { get; set; }
        public int price { get; set; }
    }

    public class ImportInventoryRes : GenericRes
    {
        public List<InventoryImportItem> items { get; set; } = new List<InventoryImportItem>();
    }

    public class InventoryImportItem
    {
        public string pattern { get; set; }
        public string machineId { get; set; }
        public string brand { get; set; }
        public int price { get; set; }
    }
}
