using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Quote
{
    public class ExportReq
    {
        public string project_name { get; set; }
        public string customer_name { get; set; }
        public string customer_address { get; set; }
        public string contact_name { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string companyTax { get; set; }
        public string fax { get; set; }
        public string create_user { get; set; }
        public string fax_type { get; set; }    // 1: 內含 2: 外加
        public List<QuoteModel> items { get; set; }
    }

}
