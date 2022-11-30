using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("member")]
    public class Member
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int salary { get; set; }
        public int annual_hours { get; set; }
        public int health_fee { get; set; }
        public int labor_fee { get; set; }
        public int welfare { get; set; }
        public int full_attendance { get; set; }
        public DateTime onboard_date { get; set; }
        public DateTime update_date { get; set; }

    }
}
