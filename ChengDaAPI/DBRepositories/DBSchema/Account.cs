using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("account")]
    public class Account
    {

        [Key]
        public string id { get; set; }
        public string account { get; set; }
        public string pwd { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public bool isActive { get; set; }
        public string token { get; set; }
        public string refreshToken { get; set; }
        public DateTime updateTime { get; set; }
    }
}
