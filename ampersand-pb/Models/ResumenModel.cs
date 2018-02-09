using ampersand.Core.Common;
using System;
using System.IO;
using System.Xml.Linq;

namespace ampersand_pb.Models
{
    public class ResumenModel: ICloneable
    {
        public string Id { get; internal set; }

        public string Periodo { get; internal set; }

        public string TextoPeriodo
        {
            get
            {
                var str = FechaDeCierre.ToString("MMMM").ToTitle();

                str += " " + FechaDeCierre.Year;

                return str;
            }
        }

        public DateTime FechaDeCierre { get; set; }

        public DateTime ProximoCierre { get; set; }

        public string FilePath { get; set; }

        public XDocument XDoc { get; internal set; }

        public TiposDeMovimiento Tipo { get; set; }

        public string Descripcion { get; set; }

        private decimal _cotizacion = 1;
        public decimal Cotizacion
        {
            get
            {
                return _cotizacion;
            }
            set
            {
                _cotizacion = value > 0 ? value : 1;
            }
        }

        public decimal Total { get; internal set; }

        public bool HuboCambios { get; internal set; }

        internal static ResumenModel GetFromFile(string file)
        {
            try
            {
                var fileName = Path.GetFileName(file);

                var resumen = new ResumenModel
                {
                    Id = fileName.Substring(0, 2),
                    Periodo = fileName.Substring(2, 6),
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
            var str = string.Format("{0} {1} {2}", Id, Descripcion, Periodo);

            return str;
        }
    }
}
