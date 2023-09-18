using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Repository.ShipDetail
{
    public class GetListReq
    {
        public List<string> orderIds { get; set; } = new List<string>();
        public List<string> inventory_ids { get; set; } = new List<string>();

    }
}
