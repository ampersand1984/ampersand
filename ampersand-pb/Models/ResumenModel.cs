using ampersand.Core.Common;
using System;
using System.IO;
using System.Xml.Linq;

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

        public DateTime ProximoCierre { get; set; }

        public string FilePath { get; set; }

        public XDocument XDoc { get; internal set; }

        public TipoMovimiento Tipo { get; set; }

        public string Descripcion { get; set; }

        public decimal Total { get; internal set; }

        public bool HuboCambios { get; internal set; }

        internal static ResumenModel GetFromFile(string file)
        {
            try
            {
                var strFechaDeCierre = Path.GetFileNameWithoutExtension(file);

                var fechaDeCierre = strFechaDeCierre.Substring(2).ToDateTime();

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

        public override string ToString()
        {
            var str = string.Format("{0} {1}", Descripcion, Periodo);

            return str;
        }
    }
}
