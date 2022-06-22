using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("system_parameter")]
    public class SystemParameter
    {
        [Key]
        public int id { get; set; }
        public string type { get; set; }
        public string name { get; set; }

    }
}
