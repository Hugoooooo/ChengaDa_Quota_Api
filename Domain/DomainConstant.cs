using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class GenericResType
    {
        //各種狀態錯誤
        public const string PARAMETER_ERROR = "參數錯誤";
        public const string CONVERT_ERROR = "轉型錯誤";
        public const string PERMISSION_DENY = "權限不足";
        public const string ITEM_EQUALS_ZERO = "未傳任何項目";
        public const string ITEM_QUANTITY_NOT_FIT = "數量不正確";
        public const string ITEM_NOT_FOUND = "查無此資料";
        public const string FILE_NOT_EXIST = "檔案不存在";
        public const string ITEM_DISABLED = "此資料已停用";
        public const string IMAGE_SIZE_ERROR = "圖片尺寸錯誤";


        public const string LOGIN_FAIL = "登入失敗"; //exception 篩選條件
        public const string LOGIN_SUCCESS = "登入成功";
        //POST
        public const string INSERT_FAIL = "新增失敗"; //exception 篩選條件
        public const string INSERT_SUCCESS = "新增成功";

        //GET
        public const string QUERY_FAIL = "查詢失敗"; //exception 篩選條件
        public const string QUERY_SUCCESS = "查詢成功";

        //PUT
        public const string UPDATE_FAIL = "更新失敗";//exception 篩選條件
        public const string UPDATE_SUCCESS = "更新成功";

        //DELETE
        public const string DELETE_FAIL = "刪除失敗";//exception 篩選條件
        public const string DELETE_SUCCESS = "刪除成功";

        //STOP
        public const string STOP_FAIL = "停用失敗";//exception 篩選條件
        public const string STOP_SUCCESS = "停用成功";

        //ACTIVE
        public const string ACTIVE_FAIL = "啟用失敗";//exception 篩選條件
        public const string ACTIVE_SUCCESS = "啟用成功";

        //PREVIEW
        public const string PREVIEW_FAIL = "預覽失敗";//exception 篩選條件
        public const string PREVIEW_SUCCESS = "預覽成功";

        //EXPORT
        public const string EXPORT_FAIL = "匯出失敗";//exception 篩選條件
        public const string EXPORT_SUCCESS = "匯出成功";

        public const string IMPORT_FAIL = "匯入失敗";//exception 篩選條件
        public const string IMPORT_SUCCESS = "匯入成功";

        public const string JOB_FAIL = "排程失敗";//exception 篩選條件
        public const string JOB_SUCCESS = "排程成功";


        //DB錯誤
        public const string DB_QUERY_NOT_FOUND = "資料庫查無此資料";
        public const string DB_QUERY_CNT_ERROR = "資料庫查詢數量不正確";
        public const string DB_UPDATE_CNT_ERROR = "資料庫更新數量不正確";
        public const string DB_ERROR = "存取DB發生錯誤";

        public const string CANCEL_FAIL = "取消失敗";//exception 篩選條件
        public const string CANCEL_SUCCESS = "取消成功";
    }

    public class GenericDBType
    {
        public const string EXPORT_PREFIX = "cdp_export";
        public const string YES = "1";
        public const string NO = "0";
    }
}
