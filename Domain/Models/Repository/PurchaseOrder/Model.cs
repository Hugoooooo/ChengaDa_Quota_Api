﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Repository.PurchaseOrder
{
    public class GetListReq
    {
        public string sDate { get; set; }
        public string eDate { get; set; }
        public string id { get; set; }
    }
}
