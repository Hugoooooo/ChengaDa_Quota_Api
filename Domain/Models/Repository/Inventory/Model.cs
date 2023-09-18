using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Repository.Inventory
{

    public class GetListReq
    {
        public string brand { get; set; }
        public string pattern { get; set; }
        public string status { get; set; }
        public string machineId { get; set; }
        public List<string> machineIds { get; set; } = new List<string>();
        public List<string> ids { get; set; } = new List<string>();

    }
}
