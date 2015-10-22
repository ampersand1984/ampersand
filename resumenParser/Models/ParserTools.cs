using System;

namespace ResumenParser.Models
{
    public static class ParserTools
    {  
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

		public static string GetPeriodo (string str)
		{
			var strYear = str.Substring (0, 4);
			var year = int.Parse (strYear);
			var strMonth = str.Substring (4, 2);
			var month = int.Parse (strMonth);

			return year.ToString () + month.ToString ("00");
		}
    }
}
