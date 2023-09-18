using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChengDaApi.Helper
{
    public static partial class Utils
    {
        /// <summary>
        /// 產生UUID
        /// </summary>
        /// <returns>回傳UUID</returns>
        public static string GenUUID()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public static int GenNumberUID()
        {
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);
            return uniqueId;
        }

        public static string GenMixedUID()
        {
            return RandomString(3) + GenNumberUID();
        }

        public static string GenInvoiceNumber(string invoice)
        {
            // 使用正規表達式找到 Invoice 字串中的數字部分
            // 假設 Invoice 格式為 "PY" 加上八個數字
            var regex = new Regex(@"\d{8}");
            var match = regex.Match(invoice);

            if (match.Success)
            {
                var numberString = match.Value;
                // 將字串轉換為數字並加一
                int number = int.Parse(numberString) + 1;
                // 根據原有的數字部分長度，補零並組合成新的數字字串
                var newNumberString = number.ToString().PadLeft(numberString.Length, '0');
                // 將新的數字字串替換回原有的 Invoice 字串中
                var newInvoice = regex.Replace(invoice, newNumberString);
                return newInvoice;
            }
            else
            {
                // 若找不到數字部分，表示 Invoice 格式不符合預期，回傳原本的值
                return invoice;
            }
        }

        public static String RandomString(int length)
        {
            //少了英文的IO和數字10，要避免使用者判斷問題時會使用到
            string allChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            //string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";//26個英文字母
            char[] chars = new char[length];
            Random rd = new Random(Guid.NewGuid().GetHashCode());
            rd.Next();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allChars[rd.Next(0, allChars.Length)];
            }

            return new string(chars);
        }

        /// <summary>
        /// 信箱檢查
        /// </summary>
        public static bool CheckEmail(string strIn)
        {
            if (strIn != "")
                return Regex.IsMatch(strIn, "^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$");
            return false;
        }

        /// <summary>
        /// 手機格式檢查
        /// </summary>
        public static bool CheckCellPhone(string strIn)
        {
            bool flag = false;
            if (strIn.Replace(" ", string.Empty) != "")
            {
                try
                {
                    long int64 = Convert.ToInt64(strIn);
                    if ((strIn.Length == 10 && strIn.StartsWith("09")) && (int64.ToString().Length == 9 && int64.ToString().StartsWith("9")))
                        flag = true;
                }
                catch { }
            }
            return flag;
        }

        /// <summary>
        ///  Deep Clone
        /// </summary>
        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        /// <summary>
        /// 計算時間間隔
        /// </summary>
        /// <param name="dtStart">時間間隔[起]</param>
        /// <param name="dtEnd">時間間隔[迄]</param>
        /// <returns>n天n小時n分鐘n秒</returns>
        public static string CountDtDiff(DateTime dtStart, DateTime dtEnd)
        {
            TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
            TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
            TimeSpan ts = tsStart.Subtract(tsEnd).Duration();
            string? dateDiff = ts.Days.ToString() + "天" + ts.Hours.ToString() + "小時" + ts.Minutes.ToString() + "分鐘" + ts.Seconds.ToString() + "秒";
            return dateDiff;
        }

        /// <summary>
        /// 首個字母轉小寫
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TranformCamelCase(string str)
        {
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        /// <summary>
        /// 日期轉換(無限期判斷)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string TransformPermanentData(DateTime date)
        {
            var endDate = new DateTime(9999, 1, 1);
            return (date > endDate) ? "99999999" : date.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 判斷日期是否生效 (適用於結束日期有99999999的情況)
        /// </summary>
        /// <returns></returns>
        public static bool CheckVaildDateIsEffective(DateTime startDate, DateTime endDate, DateTime now)
        {
            if (endDate > new DateTime(9999, 1, 1))
            {
                return now >= startDate;
            }
            else
            {
                return now >= startDate && now <= endDate;
            }
        }

     


    }
}
