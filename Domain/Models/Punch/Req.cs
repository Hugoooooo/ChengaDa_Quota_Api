using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Quote
{
    public class UploadPunchDetailReq
    {
        public string date { get; set; }
        public IFormFile file { set; get; }

    }

    public class InsertDayoffReq
    {
        public int memberId { get; set; }
        public string type { get; set; }
        public bool isAllDay { get; set; }
        public string offDate { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }

    }


    public class InsertPunchDetailReq
    {
        public InsertPunchDetailReq()
        {
            items = new List<PunchDetailModel>();
        }
        public int memberId { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public List<PunchDetailModel> items { get; set; }
    }


    public class PunchDetailModel
    {
        public string punchDate { get; set; }
        public string onWork { get; set; }
        public string offWork { get; set; }
        public bool isHoliday { get; set; }
        public bool isFillDay { get; set; }
    }

    public class CalcSalaryReq
    {
        public int memberId { get; set; }
        public int year { get; set; }
        public int month { get; set; }
    }
}
