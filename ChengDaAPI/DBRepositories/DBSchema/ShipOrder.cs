using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("ship_order")]
    public class ShipOrder
    {
        public const string TYPE_SHP = "出貨";
        public const string TYPE_OC = "他地代收";
        public const string TYPE_TST = "他人自載";

        [Key]
        public string id { get; set; }
        public string type { get; set; }
        public string customer { get; set; }
        public int amount { get; set; }
        public string invoice { get; set; }
        public string tax_type { get; set; }
        public string note { get; set; }
        public string create_member { get; set; }
        public DateTime create_date { get; set; }
        public DateTime ship_date { get; set; }
        public string update_member { get; set; }
        public DateTime update_date { get; set; }

    }
}
