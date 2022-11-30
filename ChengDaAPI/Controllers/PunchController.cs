using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ChengDaApi.Controllers.Job;
using ChengDaApi.DBRepositories.DBSchema;
using ChengDaApi.DBRepositories.IRepositories;
using DinkToPdf;
using DinkToPdf.Contracts;
using Domain;
using Domain.Models;
using Domain.Models.Quote;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace ChengDaApi.Controllers
{
    [ApiController]
    [Route("PunchService")]
    public class PunchController : ControllerBase
    {

        private readonly ILogger<PunchController> _logger;
        private readonly IMapper _mapper;
        private readonly IDatabase _database;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IMemberRepository _memberRepository;
        private readonly IPunchDetailRepository _punchDetailRepository;
        private readonly IDayoffDetailRepository _dayoffDetailRepository;



        public PunchController(ILogger<PunchController> logger, IConverter pdfConverter, Microsoft.Extensions.Configuration.IConfiguration configuration, IMemberRepository memberRepository,
            IPunchDetailRepository punchDetailRepository, IDayoffDetailRepository dayoffDetailRepository, IMapper mapper, IDatabase database)
        {
            _logger = logger;
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper;
            _database = database;
            _memberRepository = memberRepository;
            _punchDetailRepository = punchDetailRepository;
            _dayoffDetailRepository = dayoffDetailRepository;
        }

        [HttpGet]
        [Route("GetMemberList")]
        public async Task<GetMemberListRes> GetMemberList()
        {
            GetMemberListRes res = new GetMemberListRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var members = await _memberRepository.GetAll();
                var dayoffs = await _dayoffDetailRepository.GetByType(DayoffDetail.TYPE_ANNUAL);
                foreach (var member in members)
                {
                    res.items.Add(new()
                    {
                        id = member.id,
                        salary = member.salary,
                        annual_hours = member.annual_hours,
                        full_attendance = member.full_attendance,
                        health_fee = member.health_fee,
                        labor_fee = member.labor_fee,
                        name = member.name,
                        onboard_date = member.onboard_date,
                        update_date = member.update_date,
                        welfare = member.welfare,
                        used_mins = dayoffs.Where(p => p.memberId == member.id).ToList().Sum(p => p.used_minute)
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("InsertDayoff")]
        public async Task<GenericRes> InsertDayoff(InsertDayoffReq req)
        {
            GenericRes res = new GenericRes()
            {
                message = GenericResType.INSERT_SUCCESS
            };

            try
            {
                await _dayoffDetailRepository.Insert(new DayoffDetail()
                {
                    memberId = req.memberId,
                    category = req.type,
                    dayoff_date = req.offDate,
                    start_date = req.startDate,
                    end_date = req.endDate,
                    used_minute = req.isAllDay? 480 :(int)(req.endDate - req.startDate).TotalMinutes
                });
                await _database.SaveChangedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.INSERT_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("GetDayoffList")]
        public async Task<GetDayoffListRes> GetDayoffList()
        {
            GetDayoffListRes res = new GetDayoffListRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var members = await _memberRepository.GetAll();
                var dayoffList = await _dayoffDetailRepository.GetAll();
                foreach (var item in dayoffList)
                {
                    res.items.Add(new DayoffListModel()
                    {
                        id = item.id,
                        memberId = item.memberId,
                        minutes = item.used_minute,
                        startTime = item.start_date,
                        endTime = item.end_date,
                        memberName = members.FirstOrDefault(p => p.id == item.memberId).name,
                        type = item.category,
                        dayoffDate = item.dayoff_date
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPut]
        [Route("RemoveDayoff")]
        public async Task<GenericRes> RemoveDayoff(int offId)
        {
            GenericRes res = new GenericRes()
            {
                message = GenericResType.DELETE_SUCCESS
            };

            try
            {
                var removeItem = await _dayoffDetailRepository.GetById(offId);
                if (removeItem == null)
                {
                    res.isError = true;
                    res.message = GenericResType.ITEM_NOT_FOUND;
                    return res;
                }
                _dayoffDetailRepository.Remove(removeItem);
                await _database.SaveChangedAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.DELETE_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("InsertPunchDetail")]
        public async Task<GenericRes> InsertPunchDetail(InsertPunchDetailReq req)
        {
            GenericRes res = new GenericRes()
            {
                message = GenericResType.INSERT_SUCCESS
            };

            try
            {
                DateTime now = DateTime.Now;
                int lastDay = DateTime.DaysInMonth(req.year, req.month);
                DateTime startDate = new DateTime(req.year, req.month, 1);
                DateTime endDate = new DateTime(req.year, req.month, lastDay);
                var existPunchs = await _punchDetailRepository.GetMonthDetailByMemberId(req.memberId, startDate, endDate);
                List<DateTime> addPunchDateList = new List<DateTime>();
                List<PunchDetail> reqItems = new List<PunchDetail>();
                foreach (var item in req.items)
                {
                    int over33Mins = 0;
                    int over66Mins = 0;
                    int regularMins = 0;
                    bool isDelay = false;
                    DateTime lunchLimit = new DateTime(2022, 1, 1, 12, 0, 0);
                    DateTime startLimit = new DateTime(2022, 1, 1, 8, 0, 0);    // 8:00
                    DateTime endLimit = new DateTime(2022, 1, 1, 17, 0, 0);     // 17:00
                    DateTime overLimit = new DateTime(2022, 1, 1, 18, 0, 0);     // 18:00
                    DateTime over66Limit = new DateTime(2022, 1, 1, 19, 0, 0);     // 19:00
                    DateTime onWork = new DateTime(2022, 1, 1, int.Parse(item.onWork.Substring(0, 2)), int.Parse(item.onWork.Substring(2)), 0);
                    DateTime offWork = new DateTime(2022, 1, 1, int.Parse(item.offWork.Substring(0, 2)), int.Parse(item.offWork.Substring(2)), 0);
                    if (item.isHoliday)
                    {
                        #region 假日
                        int diffMins = (int)(offWork - onWork).TotalMinutes;
                        bool haveLunchTime = onWork < lunchLimit && offWork > lunchLimit;
                        diffMins = haveLunchTime ? diffMins - 60 : diffMins;
                        over66Mins = diffMins;
                        #endregion
                    }
                    else
                    {
                        #region 平日
                        if (onWork < startLimit)
                        {
                            onWork = startLimit;
                        }
                        else if (onWork > startLimit)
                        {
                            isDelay = true;
                        }

                        if (offWork > overLimit)
                        {
                            // 加班
                            regularMins = (int)(endLimit - onWork).TotalMinutes;
                            if (offWork > over66Limit)
                            {
                                over66Mins = (int)(offWork - over66Limit).TotalMinutes;
                                over33Mins = (int)(over66Limit - endLimit).TotalMinutes;

                            }
                            else
                            {
                                over33Mins = (int)(offWork - endLimit).TotalMinutes;
                            }
                        }
                        else if (offWork > endLimit)
                        {
                            // 超過下班時間但未達加班時間
                            regularMins = (int)(endLimit - onWork).TotalMinutes;
                        }
                        else
                        {
                            regularMins = (int)(offWork - onWork).TotalMinutes;
                        }
                        #endregion
                    }

                    reqItems.Add(new PunchDetail()
                    {
                        member_id = req.memberId,
                        on_work = item.onWork,
                        off_work = item.offWork,
                        over33_total = over33Mins,
                        over66_total = over66Mins,
                        regular_total = regularMins,
                        punch_date = DateTime.Parse(item.punchDate),
                        is_holiday = item.isHoliday,
                        is_delay = isDelay,
                        create_time = now,
                    });

                    addPunchDateList.Add(DateTime.Parse(item.punchDate));
                }
                var deleteItems = existPunchs.Where(p => addPunchDateList.Contains(p.punch_date)).ToList();
                _punchDetailRepository.RemoveRange(deleteItems);
                await _punchDetailRepository.AddRange(reqItems);
                await _database.SaveChangedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.INSERT_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.INSERT_FAIL;
            }

            return res;
        }

        [HttpGet]
        [Route("GetPunchList")]
        public async Task<GetPunchListRes> GetPunchList(int memberId, int year,int month)
        {
            GetPunchListRes res = new GetPunchListRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var members = await _memberRepository.GetAll();
                int lastDay = DateTime.DaysInMonth(year, month);
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = new DateTime(year, month, lastDay);
                var punchList = await _punchDetailRepository.GetMonthDetailByMonth(startDate, endDate);
                if(memberId > 0)
                {
                    punchList = punchList.Where(p => p.member_id == memberId).ToList();
                }
                foreach (var item in punchList)
                {
                    res.items.Add(new PunchListModel()
                    {
                        id = item.id,
                        memberId = item.member_id,
                        memberName = members.Find(p => p.id == item.member_id).name,
                        onWork = item.on_work,
                        offWrok = item.off_work,
                        punchDate = item.punch_date,
                        regularTotal = item.regular_total,
                        over33Total = item.over33_total,
                        over66Total = item.over66_total
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

        [HttpPost]
        [Route("CalcSalary")]
        public async Task<CalcSalaryRes> CalcSalary(CalcSalaryReq req)
        {
            CalcSalaryRes res = new CalcSalaryRes()
            {
                message = GenericResType.QUERY_SUCCESS
            };

            try
            {
                var member = await _memberRepository.GetById(req.memberId);
                int lastDay = DateTime.DaysInMonth(req.year, req.month);
                DateTime startDate = new DateTime(req.year, req.month, 1);
                DateTime endDate = new DateTime(req.year, req.month, lastDay);
                var punchDatas = await _punchDetailRepository.GetMonthDetailByMemberId(req.memberId, startDate, endDate);
                var dayoffDatas = await _dayoffDetailRepository.GetDetailByMemberId(req.memberId, startDate, endDate);
                var annualDayoffDatas = await _dayoffDetailRepository.GetDetailByMemberId(req.memberId, member.onboard_date, member.onboard_date.AddYears(1));
                // 800 -1700 基本9HR
                // 判斷遲到3次扣全勤
                // 10HR 以上算加班  10-12 => 1.33  12以上 => 1.66 + 100餐費
                double minSalary = (double)member.salary / 30 / 8 / 60;
                int over33Mins = punchDatas.Sum(p => p.over33_total);
                int over66Mins = punchDatas.Sum(p => p.over66_total);
                double over33Total = over33Mins * 1.33 * minSalary;
                double over66Total = over66Mins * 1.66 * minSalary;
                int over66Days = punchDatas.Where(p => p.over66_total > 0 && !p.is_holiday).ToList().Count;
                int delayDays = punchDatas.Where(p => p.is_delay).ToList().Count;
                int fullAttenance = 0;
                if (delayDays == 0)
                    fullAttenance = member.full_attendance;
                else if (delayDays < 3)
                    fullAttenance = member.full_attendance - (member.full_attendance / 3) * delayDays;

                res.memberId = req.memberId;
                res.memberName = member.name;
                res.regularTotal = punchDatas.Sum(p => p.regular_total);
                res.basicSalary = member.salary;
                res.delayCount = delayDays;
                res.healthFee = member.health_fee;
                res.laborFee = member.labor_fee;
                res.welfare = member.welfare;
                res.fullAttendance = member.full_attendance;
                res.overtimeMins = punchDatas.Sum(p => p.over33_total + p.over66_total);
                res.overtimeSalary = (int)Math.Round(over33Total + over66Total);
                res.overtimeBonus = 100 * over66Days;
                res.annualDayoffMins = dayoffDatas.Where(p => p.category == DayoffDetail.TYPE_ANNUAL).Sum(p => p.used_minute);
                res.sickDayoffMins = dayoffDatas.Where(p => p.category == DayoffDetail.TYPE_SICK).Sum(p => p.used_minute);
                res.personalDayoffMins = dayoffDatas.Where(p => p.category == DayoffDetail.TYPE_PERSONAL).Sum(p => p.used_minute);
                res.annualHours = member.annual_hours;
                res.usedAnnualMins = annualDayoffDatas.Where(p => p.category == DayoffDetail.TYPE_ANNUAL).Sum(p => p.used_minute);
                // 底薪 + 獎金 + 全勤 + 加班費 + 加班獎金 - 勞健保 - 病假 - 事假
                int personalPrice = (int)Math.Round(res.personalDayoffMins * minSalary);
                int sickPrice = (int)Math.Round(res.sickDayoffMins * minSalary / 2);
                res.monthSalary = res.basicSalary + res.welfare + res.fullAttendance + res.overtimeSalary + res.overtimeBonus - res.laborFee - res.healthFee - personalPrice - sickPrice;

            }
            catch (Exception ex)
            {
                _logger.LogError(GenericResType.QUERY_FAIL + " ex:" + ex.ToString());

                res.isError = true;
                res.message = GenericResType.QUERY_FAIL;
            }

            return res;
        }

    }
}
