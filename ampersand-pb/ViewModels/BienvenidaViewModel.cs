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
        public const int ULTIMOS_GASTOS_CANTIDAD = 7;

        public BienvenidaViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;
        }

        private readonly IMovimientosDataAccess _movimientosDA;
        private readonly ConfiguracionModel _configuracionM;

        private SeleccionDeMediosDePagoViewModel _seleccionDeMediosDePagoVM;
        public SeleccionDeMediosDePagoViewModel SeleccionDeMediosDePagoVM
        {
            get
            {
                if (_seleccionDeMediosDePagoVM == null)
                {
                    _seleccionDeMediosDePagoVM = new SeleccionDeMediosDePagoViewModel(_configuracionM);
                    _seleccionDeMediosDePagoVM.SeleccionDeMediosDePagoCambiada += SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada;
                }
                return _seleccionDeMediosDePagoVM;
            }
        }

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
        private void SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada(object sender, EventArgs e)
        {
            _ultimos_Gastos = null;
            _ultimas_Cuotas = null;
            _totales = null;
            OnPropertyChanged("Ultimos_Gastos");
            OnPropertyChanged("Ultimas_Cuotas");
            OnPropertyChanged("Totales");
        }

        private IEnumerable<BaseModel> GetUltimosGastos()
        {
            var resumenAgrupado = _movimientosDA.GetUltimoResumen();
            var movimientos = _movimientosDA.GetMovimientos(resumenAgrupado);
            
            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            movimientos = movimientos.Where(a => mediosDePago.Contains(a.IdResumen));

            var result = movimientos.Where(a => !a.EsMensual && a.Fecha.GetPeriodo() == resumenAgrupado.Periodo)
                .OrderByDescending(a => a.Fecha)
                .Take(ULTIMOS_GASTOS_CANTIDAD);

            return result;
        }

        private IEnumerable<BaseModel> GetUltimasCuotas()
        {
            var resumenAgrupado = _movimientosDA.GetUltimoResumen();
            var movimientos = _movimientosDA.GetMovimientos(resumenAgrupado);

            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            movimientos = movimientos.Where(a => mediosDePago.Contains(a.IdResumen));

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

            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            movimientosActuales = movimientosActuales.Where(a => mediosDePago.Contains(a.IdResumen));

            var movimientosMesProximo = MovimientosViewModel.GetProyeccion(movimientosActuales);

            var textoPeriodoMesProximo = DateTime.ParseExact(resumenActual.Periodo + "01", "yyyyMMdd", CultureInfo.InvariantCulture)
                .AddMonths(1)
                .ToString("MMMM yyyy")
                .ToTitle();

            return new List<AgrupacionItem>()
            {
                new AgrupacionItem()
                {
                    Descripcion = resumenAnterior.TextoPeriodo,
                    Monto = resumenAnterior.Resumenes.Where(a => mediosDePago.Contains(a.Id)).Sum(b => b.Total)
                },
                new AgrupacionItem()
                {
                    Descripcion = resumenActual.TextoPeriodo,
                    Monto = resumenActual.Resumenes.Where(a => mediosDePago.Contains(a.Id)).Sum(b => b.Total)
                },
                new AgrupacionItem()
                {
                    Descripcion = textoPeriodoMesProximo,
                    Monto = movimientosMesProximo.Sum(a => a.Monto)
                }
            };
        }
    }
}
