using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("dayoff_detail")]
    public class DayoffDetail
    {
        public const string TYPE_ANNUAL = "特休";
        public const string TYPE_SICK = "病假";
        public const string TYPE_PERSONAL = "事假";

        [Key]
        public int id { get; set; }
        public int memberId { get; set; }
        public string category { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public DateTime dayoff_date { get; set; }
        public int used_minute { get; set; }


    }
}
