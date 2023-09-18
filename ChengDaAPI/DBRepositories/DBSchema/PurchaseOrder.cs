using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("purchase_order")]
    public class PurchaseOrder
    {
        public const string TYPE_GP = "進貨";
        public const string TYPE_OD = "他地代送";
        public const string TYPE_ST = "自載";

        [Key]
        public string id { get; set; }
        public string type { get; set; }
        public string note { get; set; }
        public DateTime purchase_date { get; set; }
        public string create_member { get; set; }
        public DateTime create_date { get; set; }
        public string update_member { get; set; }
        public DateTime update_date { get; set; }
    }
}
