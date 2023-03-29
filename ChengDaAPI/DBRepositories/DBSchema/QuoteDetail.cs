using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ChengDaApi.DBRepositories.DBSchema
{
    [Table("quote_detail")]
    public class QuoteDetail
    {
        [Key]
        public int id { get; set; }
        public int qid { get; set; }
        public string product { get; set; }
        public int quantity { get; set; }
        public string pattern { get; set; }
        public int unit_price { get; set; }
        public int amount { get; set; }
        public string remark { get; set; }
        public int idx { get; set; }
        public string unit { get; set; }
    }
}
