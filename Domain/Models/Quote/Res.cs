using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Quote
{
    public class GetQuoteRes : GenericRes
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

    public class GetParametersRes : GenericRes
    {
        public List<string> datas { get; set; }
    }

    public class GetDayoffListRes : GenericRes
    {
        public GetDayoffListRes()
        {
            items = new List<DayoffListModel>();
        }
        public List<DayoffListModel> items { get; set; }
    }

    public class GetPunchListRes: GenericRes
    {
        public GetPunchListRes()
        {
            items = new List<PunchListModel>();
        }
        public List<PunchListModel> items { get; set; }
    }

    public class PunchListModel
    {
        public int id { get; set; }
        public int memberId { get; set; }
        public string memberName { get; set; }
        public DateTime punchDate { get; set; }
        public string onWork { get; set; }
        public string offWrok { get; set; }
        public int regularTotal { get; set; }
        public int over33Total { get; set; }
        public int over66Total { get; set; }
        public bool isDelay { get; set; }

    }

    public class DayoffListModel
    {
        public int id { get; set; }
        public int memberId { get; set; }
        public int minutes { get; set; }
        public string memberName { get; set; }
        public string type { get; set; }
        public DateTime dayoffDate { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

    }

    public class GetMemberListRes : GenericRes
    {
        public GetMemberListRes()
        {
            items = new List<MemberDetail>();
        }
        public List<MemberDetail> items { get; set; }
    }

    public class MemberDetail
    {
        public int used_mins { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int salary { get; set; }
        public int annual_hours { get; set; }
        public int health_fee { get; set; }
        public int labor_fee { get; set; }
        public int welfare { get; set; }
        public int full_attendance { get; set; }
        public DateTime onboard_date { get; set; }
        public DateTime update_date { get; set; }
    }

    public class CalcSalaryRes:GenericRes
    {
        public int memberId { get; set; }
        public string memberName { get; set; }
        public int regularTotal { get; set; }   // 正常上班時數
        public int basicSalary { get; set; }    //底薪
        public int delayCount { get; set; }      //遲到次數
        public int healthFee { get; set; }
        public int laborFee { get; set; }
        public int welfare { get; set; }    //獎金
        public int fullAttendance { get; set; } //全勤
        public int overtimeMins { get; set; }  //加班時數
        public int overtimeBonus { get; set; }  //加班餐費
        public int overtimeSalary { get; set; } //加班費
        public int annualDayoffMins { get; set; }   //特休假時數
        public int sickDayoffMins { get; set; }     //病假時數
        public int personalDayoffMins { get; set; }  //事假時數
        public int annualHours { get; set; }    //總特休時數
        public int usedAnnualMins { get; set; } //剩餘特休
        public int monthSalary { get; set; }    // 當月薪月



    }
}
