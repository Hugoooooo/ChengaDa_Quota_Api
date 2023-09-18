using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("inventory")]
    public class Inventory
    {
        public const string STATUS_STOCK = "庫存";
        public const string STATUS_SHIPPED = "已出貨";
        public const string STATUS_RETURN = "已退貨";

        [Key]
        public string id { get; set; }
        public string pattern { get; set; }
        public string machineId { get; set; }
        public string status { get; set; }
        public string brand { get; set; }
        public int price { get; set; }
        public string create_member { get; set; }
        public DateTime create_date { get; set; }
        public string update_member { get; set; }
        public DateTime update_date { get; set; }
    }
}
