using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.Models
{
    public static class ResumenModelExtension
    {
        public static IEnumerable<ResumenAgrupadoModel> Agrupar(this IEnumerable<ResumenModel> resumenes)
        {
            var resultado = new List<ResumenAgrupadoModel>();

            var periodos = resumenes.Select(a => a.Periodo).Distinct().OrderByDescending(a => a);

            foreach (var periodo in periodos)
            {
                var resumenAgrupadoM = new ResumenAgrupadoModel(resumenes.Where(a => a.Periodo.Equals(periodo)).ToList());
                resultado.Add(resumenAgrupadoM);
            }

            if (resultado.Any())
                resultado.First().EsElUtimoMes = true;

            return resultado;
        }
    }
}
