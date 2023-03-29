using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class GenericRes
    {
        public bool isError { set; get; }
        public string message { set; get; }
    }

    public class QuoteModel
    {
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
