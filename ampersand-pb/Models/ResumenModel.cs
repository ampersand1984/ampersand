using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ampersand.Core.Common;
using System.Globalization;

namespace ampersand_pb.Models
{
    public class ResumenModel: ICloneable
    {
        public string Periodo
        {
            get
            {
                return FechaDeCierre.GetPeriodo();
            }
        }
        public string TextoPeriodo
        {
            get
            {
                var str = FechaDeCierre.ToString("MMMM");
                str = str.Substring(0, 1).ToUpper() + str.Substring(1);

                str += " " + FechaDeCierre.Year;

                return str;
            }
        }

        public DateTime FechaDeCierre { get; set; }

        public string FilePath { get; set; }

        public bool EsElUtimoMes { get; set; }

        internal static ResumenModel GetFromFile(string file)
        {
            try
            {
                var strFechaDeCierre = Path.GetFileNameWithoutExtension(file);

                var fechaDeCierre = strFechaDeCierre.Substring(1).ToDateTime();

                var resumen = new ResumenModel
                {
                    FechaDeCierre = fechaDeCierre,
                    FilePath = file
                };

                return resumen;
            }
            catch (Exception)
            {
                //throw;
            }
            return null;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
