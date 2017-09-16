using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace ampersand.Core.Common
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this String str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool EsPeriodoValido(this String str)
        {
            var result = str.Length == 6;
            if (result)
            {
                try
                {
                    GetPeriodo(str);
                }
                catch (Exception)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFecha">en formato yyyyMMdd</param>
        /// <returns></returns>
        public static string GetPeriodo(string strFecha)
        {
            var strYear = strFecha.Substring(0, 4);
            var year = int.Parse(strYear);
            var strMonth = strFecha.Substring(4, 2);
            var month = int.Parse(strMonth);

            return year.ToString() + month.ToString("00");
        }

        public static string GetPeriodo(this DateTime fecha)
        {
            var periodo = string.Format("{0}{1}", fecha.Year, fecha.Month.ToString("00"));
            return periodo;
        }

        public static T Parse<T>(string stringValue) where T : struct, IConvertible, IComparable, IFormattable
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    return (T)converter.ConvertFromString(stringValue);
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            else
                return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strDateTime">en formato yyyyMMdd</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this String strDateTime)
        {
            var result = DateTime.MinValue;

            try
            {
                var year = int.Parse(strDateTime.Substring(0, 4));
                var month = int.Parse(strDateTime.Substring(4, 2));
                var day = int.Parse(strDateTime.Substring(6, 2));
                result = new DateTime(year, month, day);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format(@"StringExtension.ToDateTime {0}: ""{1}""", ex.GetType().Name, strDateTime));
            }

            return result;
        }

        /// <summary>
        /// https://stackoverflow.com/a/1206029
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToTitle(this string str)
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            var title = textInfo.ToTitleCase(str);

            return title;
        }
    }
}
