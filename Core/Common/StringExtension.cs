using System;
using System.ComponentModel;
using System.Diagnostics;

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

        public static string GetPeriodo(string str)
        {
            var strYear = str.Substring(0, 4);
            var year = int.Parse(strYear);
            var strMonth = str.Substring(4, 2);
            var month = int.Parse(strMonth);

            return year.ToString() + month.ToString("00");
        }

        public static string GetPeriodo(this DateTime fecha)
        {
            var periodo = fecha.Year.ToString() + fecha.Month.ToString("00");
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

        public static DateTime ToDateTime(this String strDateTime)
        {
            var result = DateTime.Today;

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
    }
}
