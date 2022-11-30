using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("punch_detail")]
    public class PunchDetail
    {
        [Key]
        public int id { get; set; }
        public int member_id { get; set; }
        public int regular_total { get; set; }
        public int over33_total { get; set; }
        public int over66_total { get; set; }
        public DateTime punch_date { get; set; }
        public string on_work { get; set; }
        public string off_work { get; set; }
        public DateTime create_time { get; set; }
        public bool is_holiday { get; set; }
        public bool is_delay { get; set; }

    }
}
