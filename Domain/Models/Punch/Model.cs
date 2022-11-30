using Microsoft.AspNetCore.Http;
using MiniExcelLibs.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Quote
{
    public class PunchRecord
    {
        //public string Name { get; set; }
        //public string Date { get; set; }
        //public string StartTime { get; set; }
        //public string EndTime { get; set; }
        [ExcelColumnWidth(20)]
        public string 姓名 { get; set; }
        [ExcelColumnWidth(20)]
        public string 日期 { get; set; }
        [ExcelColumnWidth(20)]
        public string 上班時間 { get; set; }
        [ExcelColumnWidth(20)]
        public string 下班時間 { get; set; }
    }

    public class TestRecord
    {
        public string 資料庫名稱_dbName { get; set; }
        public string 資料表名稱_tableName { get; set; }
        public string 欄位名稱_columnName { get; set; }
        public string 欄位順序_columnOrder { get; set; }
        public string 主鍵_columnPK { get; set; }
        public string 資料來源_dataSource { get; set; }
        public string 資料範例_dataSample { get; set; }
        public string 資料標籤_dataTag { get; set; }
        public string 缺值處理方式_handleWithIsNull { get; set; }
        public string 缺值原因_reasonOfIsNull { get; set; }
        public string 開始留存時間_startTime { get; set; }
        public string 停止留存時間_endTime { get; set; }
        public string 修改時間_updateTime { get; set; }

    }

}
