using System;
using System.Collections.Generic;

namespace Domain.Models.Inventory
{
    public class AddPurchaseOrderReq
    {
        public string type { get; set; }
        public string note { get; set; }
        public string purchaseDate { get; set; }
        public List<AddPurchaseDetail> items { get; set; } = new List<AddPurchaseDetail>();
    }

    public class AddPurchaseDetail
    {
        public int? id { get; set; }
        public string inventoryId { get; set; }
        public string pattern { get; set; }
        public string machineId { get; set; }
        public string brand { get; set; }
        public int? price { get; set; }
    }

    public class AddShipOrderReq
    {
        public string type { get; set; }
        public string customer { get; set; }
        public int? amount { get; set; }
        public string taxType { get; set; }
        public string invoice { get; set; }
        public string note { get; set; }
        public string shipDate { get; set; }
        public List<string> inventoryIds { get; set; } = new List<string>();
    }


    public class RemovePurchaseOrderReq
    {
        public string orderId { get; set; }
    }

    public class RemoveShipOrderReq
    {
        public string orderId { get; set; }
    }

    public class UpdatePurchaseOrderReq
    {
        public string type { get; set; }
        public string orderId { get; set; }
        public string note { get; set; }
        public string purchaseDate { get; set; }
        public List<UpdatePurchaseDetail> items { get; set; } = new List<UpdatePurchaseDetail>();
    }

    public class UpdatePurchaseDetail: AddPurchaseDetail
    {
     
    }

    public class UpdateShipOrderReq
    {
        public string orderId { get; set; }
        public string type { get; set; }
        public string customer { get; set; }
        public int? amount { get; set; }
        public string taxType { get; set; }
        public string invoice { get; set; }
        public string note { get; set; }
        public string shipDate { get; set; }
        public List<string> inventoryIds { get; set; } = new List<string>();
    }

}
