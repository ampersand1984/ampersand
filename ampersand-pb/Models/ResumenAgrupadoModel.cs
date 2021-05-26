using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.Models
{
    public class ResumenAgrupadoModel
    {
        public ResumenAgrupadoModel(IEnumerable<ResumenModel> resumenes)
        {
            Resumenes = resumenes;
        }

        public IEnumerable<ResumenModel> Resumenes { get; private set; }

        public string Periodo
        {
            get
            {
                return Resumenes.First().Periodo;
            }
        }

        public string TextoPeriodo
        {
            get
            {
                return Resumenes.First().TextoPeriodo;
            }
        }

        public bool EsElUtimoMes { get; set; }

        public object Clone()
        {
            var clone = this.MemberwiseClone() as ResumenAgrupadoModel;
            var resumenes = new List<ResumenModel>();
            foreach (var resumen in clone.Resumenes)
                resumenes.Add(resumen.Clone() as ResumenModel);

            clone.Resumenes = resumenes;

            return clone;
        }
    }
}
