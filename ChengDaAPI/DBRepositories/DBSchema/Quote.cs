using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("quote")]
    public class Quote
    {
        [Key]
        public int id { get; set; }
        public string case_number { get; set; }
        public string project_name { get; set; }
        public string customer_name { get; set; }
        public string customer_address { get; set; }
        public string contact_name { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string companyTax { get; set; }
        public string fax { get; set; }
        public string create_user { get; set; }
        public DateTime create_time { get; set; }
        public string fax_type { get; set; }
    }
}
