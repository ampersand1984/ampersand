using ampersand.Core;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.ViewModels
{
    public class BienvenidaViewModel: BaseViewModel
    {
        public const int ULTIMOS_GASTOS_CANTIDAD = 10;

        public BienvenidaViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;
        }

        private readonly IMovimientosDataAccess _movimientosDA;
        private readonly ConfiguracionModel _configuracionM;

        private IEnumerable<BaseModel> _ultimos_Gastos;
        public IEnumerable<BaseModel> Ultimos_Gastos
        {
            get => _ultimos_Gastos ?? (_ultimos_Gastos = GetUltimosGastos());
        }

        private IEnumerable<BaseModel> _ultimas_Cuotas;
        public IEnumerable<BaseModel> Ultimas_Cuotas
        {
            get => _ultimas_Cuotas ?? (_ultimas_Cuotas = GetUltimasCuotas());
        }

        private IEnumerable<BaseModel> GetUltimosGastos()
        {
            var resumenAgrupado = _movimientosDA.GetUltimoResumen();
            var movimientos = _movimientosDA.GetMovimientos(resumenAgrupado);

            var result = movimientos.Where(a => !a.EsMensual)
                .OrderByDescending(a => a.Fecha)
                .Take(ULTIMOS_GASTOS_CANTIDAD);

            return result;
        }

        private IEnumerable<BaseModel> GetUltimasCuotas()
        {
            var resumenAgrupado = _movimientosDA.GetUltimoResumen();
            var movimientos = _movimientosDA.GetMovimientos(resumenAgrupado);

            var result = movimientos.Where(a => a.CoutasPendientes != -1)
                .OrderBy(a => a. CoutasPendientes)
                .Take(ULTIMOS_GASTOS_CANTIDAD);

            return result;
        }
    }
}
