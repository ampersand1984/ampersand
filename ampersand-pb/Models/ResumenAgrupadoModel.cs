using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.Models
{
    public class ResumenAgrupadoModel
    {
        public ResumenAgrupadoModel(IEnumerable<ResumenModel> resumenes)
        {
            Resumenes = resumenes;
            Periodo = resumenes.First().Periodo;
            TextoPeriodo = resumenes.First().TextoPeriodo;
        }

        public IEnumerable<ResumenModel> Resumenes { get; private set; }

        public string Periodo { get; private set; }

        public string TextoPeriodo { get; private set; }

        public bool EsElUtimoMes { get; internal set; }
    }
}
