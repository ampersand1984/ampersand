using ampersand.Core.Common;
using System;
using System.IO;
using System.Xml.Linq;

namespace ampersand_pb.Models
{
    public class ResumenModel : ICloneable
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

        public decimal GetTotal(bool incluyeAjenos = true)
        {
            return GetTotal(null, incluyeAjenos);
        }
        public decimal GetTotalDeuda(bool incluyeAjenos = true)
        {
            return GetTotal("Deuda", incluyeAjenos);
        }
        public decimal GetTotalSinDeuda(bool incluyeAjenos = true)
        {
            return GetTotal("Mov", incluyeAjenos);
        }

        private decimal GetTotal(string elementsName, bool incluyeAjenos)
        {
            var total = 0.00M;
            var attrCotiz = XDoc.Root.Attribute("Cotizacion");

            var elements = elementsName.IsNullOrEmpty() ?
                XDoc.Root.Elements() :
                XDoc.Root.Elements(elementsName);

            foreach (var mov in elements)
            {
                if (!incluyeAjenos)
                {
                    var attrEsAjeno = mov.Attribute("EsAjeno");
                    if (attrEsAjeno != null)
                    {
                        var esAjeno = false;
                        bool.TryParse(attrEsAjeno.Value, out esAjeno);
                        if (esAjeno)
                            continue;
                    }
                }

                var cotiz = 1.00M;
                var attrMonto = mov.Attribute("Monto");
                if (attrMonto == null)
                {
                    attrMonto = mov.Attribute("MontoME");

                    decimal.TryParse(attrCotiz.Value, out cotiz);
                }

                var strMonto = attrMonto.Value;

                var monto = 0.00M;
                decimal.TryParse(strMonto, out monto);
                total += monto * cotiz;
            }
            return total;
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
