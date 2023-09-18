using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("purchase_detail")]
    public class PurchaseDetail
    {

        [Key]
        public int id { get; set; }
        public string order_id { get; set; }
        public string inventory_id { get; set; }
        public string create_member { get; set; }
        public DateTime create_date { get; set; }
        public string update_member { get; set; }
        public DateTime update_date { get; set; }
    }
}
