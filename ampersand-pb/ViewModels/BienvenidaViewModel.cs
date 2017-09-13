using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private IEnumerable<AgrupacionItem> _totales;
        public IEnumerable<AgrupacionItem> Totales
        {
            get
            {
                return _totales ?? (_totales = GetTotales());
            }
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

        private IEnumerable<AgrupacionItem> GetTotales()
        {
            var resumenActual = _movimientosDA.GetUltimoResumen();

            var periodoAnterior = DateTime.ParseExact(resumenActual.Periodo + "01", "yyyyMMdd", CultureInfo.InvariantCulture)
                .AddMonths(-1)
                .GetPeriodo();

            var resumenAnterior = _movimientosDA.GetResumen(periodoAnterior);

            var movimientosActuales = _movimientosDA.GetMovimientos(resumenActual);

            var movimientosMesProximo = MovimientosViewModel.GetProyeccion(movimientosActuales);

            var textoPeriodoMesProximo = DateTime.ParseExact(resumenActual.Periodo + "01", "yyyyMMdd", CultureInfo.InvariantCulture)
                .AddMonths(1)
                .ToString("MMMM yyyy")
                .ToTitle();

            return new List<AgrupacionItem>()
            {
                new AgrupacionItem() { Descripcion = resumenAnterior.TextoPeriodo, Monto = resumenAnterior.Resumenes.Sum(a => a.Total) },
                new AgrupacionItem() { Descripcion = resumenActual.TextoPeriodo, Monto = resumenActual.Resumenes.Sum(a => a.Total) },
                new AgrupacionItem() { Descripcion = textoPeriodoMesProximo, Monto = movimientosMesProximo.Sum(a => a.Monto) }
            };
        }
    }
}
