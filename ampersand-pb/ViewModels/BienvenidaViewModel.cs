using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using Serilog;
using static ampersand_pb.ViewModels.MovimientosViewModel;

namespace ampersand_pb.ViewModels
{
    public class BienvenidaViewModel : BaseViewModel
    {
        public const int ULTIMOS_GASTOS_CANTIDAD = 7;

        public BienvenidaViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            Log.Information("BienvenidaViewModel start");
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

        private IEnumerable<BaseMovimiento> _ultimos_Gastos;
        public IEnumerable<BaseMovimiento> Ultimos_Gastos
        {
            get
            {
                if (_ultimos_Gastos == null)
                    CargarUltimosGastos();
                return _ultimos_Gastos;
            }
        }

        public bool HayUltimosGastos
        {
            get
            {
                return _ultimos_Gastos != null;
            }
        }

        private IEnumerable<BaseModel> _ultimas_Cuotas;
        public IEnumerable<BaseModel> Ultimas_Cuotas
        {
            get
            {
                if (_ultimas_Cuotas == null)
                    CargarUltimasCuotas();
                return _ultimas_Cuotas;
            }
        }

        private IEnumerable<AgrupacionItem> _totales;
        public IEnumerable<AgrupacionItem> Totales
        {
            get
            {
                if (_totales == null)
                    CargarTotales();
                return _totales;
            }
        }

        private IEnumerable<AgrupacionItem> _totalesDelMesActual;
        public IEnumerable<AgrupacionItem> TotalesDelMesActual
        {
            get
            {
                if (_totalesDelMesActual == null)
                    CargarTotalesDelMesActual();
                return _totalesDelMesActual;
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

        private void CargarUltimosGastos()
        {
            var task = new Task<IEnumerable<BaseMovimiento>>(() =>
            {
                Log.Information("BienvenidaViewModel GetUltimosGastos start");

                var resumenAgrupado = _movimientosDA.GetUltimoResumen();
                var movimientos = _movimientosDA.GetMovimientos(resumenAgrupado);

                var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

                movimientos = movimientos.Where(a => mediosDePago.Contains(a.IdResumen));

                var result = movimientos.Where(a => !a.EsMensual/* && a.Fecha.GetPeriodo() == resumenAgrupado.Periodo*/)
                    .OrderByDescending(a => a.Fecha)
                    .Take(ULTIMOS_GASTOS_CANTIDAD);

                Log.Information("BienvenidaViewModel GetUltimosGastos end");
                return result;
            });

            task.ContinueWith(t =>
            {
                _ultimos_Gastos = t.Result;
                OnPropertyChanged("Ultimos_Gastos");
            });

            task.Start();
        }

        private void CargarUltimasCuotas()
        {
            var task = new Task<IEnumerable<BaseModel>>(() =>
            {
                Log.Information("BienvenidaViewModel GetUltimasCuotas start");

                var resumenAgrupado = _movimientosDA.GetUltimoResumen();
                var movimientos = _movimientosDA.GetMovimientos(resumenAgrupado);

                var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

                movimientos = movimientos.Where(a => mediosDePago.Contains(a.IdResumen));

                var result = movimientos.Where(a => a.CoutasPendientes != -1)
                    .OrderBy(a => a.CoutasPendientes)
                    .Take(ULTIMOS_GASTOS_CANTIDAD);

                Log.Information("BienvenidaViewModel GetUltimasCuotas end");
                return result;
            });

            task.ContinueWith(t =>
            {
                _ultimas_Cuotas = t.Result;
                OnPropertyChanged("Ultimas_Cuotas");
            });

            task.Start();
        }

        private void CargarTotales()
        {
            var task = new Task<IEnumerable<AgrupacionItem>>(() =>
            {
                Log.Information("BienvenidaViewModel GetTotales start");

                var resumenActual = _movimientosDA.GetUltimoResumen();

                var periodoAnterior = DateTime.ParseExact(resumenActual.Periodo + "01", "yyyyMMdd", CultureInfo.InvariantCulture)
                    .AddMonths(-1)
                    .GetPeriodo();

                var resumenAnterior = _movimientosDA.GetResumen(periodoAnterior);

                var movimientosActuales = _movimientosDA.GetMovimientos(resumenActual);

                var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

                movimientosActuales = movimientosActuales.Where(a => mediosDePago.Contains(a.IdResumen));

                var movimientosMesProximo = MovimientosViewModel.GetMovimientosProyectados(movimientosActuales);

                var textoPeriodoMesProximo = DateTime.ParseExact(resumenActual.Periodo + "01", "yyyyMMdd", CultureInfo.InvariantCulture)
                    .AddMonths(1)
                    .ToString("MMMM yyyy")
                    .ToTitle();

                var totales = new List<AgrupacionItem>()
            {
                new AgrupacionItem()
                {
                    Descripcion = resumenAnterior.TextoPeriodo,
                    Monto = resumenAnterior.Resumenes.Where(a => mediosDePago.Contains(a.Id)).Sum(b => b.GetTotal())
                },
                new AgrupacionItem()
                {
                    Descripcion = resumenActual.TextoPeriodo,
                    Monto = resumenActual.Resumenes.Where(a => mediosDePago.Contains(a.Id)).Sum(b => b.GetTotal())
                },
                new AgrupacionItem()
                {
                    Descripcion = textoPeriodoMesProximo,
                    Monto = movimientosMesProximo.Sum(a => a.Monto)
                }
            };

                Log.Information("BienvenidaViewModel GetTotales end");
                return totales;
            });
            task.ContinueWith(t =>
            {
                _totales = t.Result;
                OnPropertyChanged("Totales");
            });
            task.Start();
        }

        private void CargarTotalesDelMesActual()
        {
            var task = new Task<IEnumerable<AgrupacionItem>>(() =>
            {
                var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

                var totales = new List<AgrupacionItem>();

                var resumenActual = _movimientosDA.GetUltimoResumen();

                foreach (var resumen in resumenActual.Resumenes.Where(a => mediosDePago.Contains(a.Id)))
                    totales.Add(new AgrupacionItem() { Tipo = TiposDeAgrupacion.MedioDePago, Id = resumen.Id, Descripcion = $"{resumen.Descripcion}{Environment.NewLine}{resumen.FechaDeCierre.ToString("dd/MM/yyyy")}", Monto = resumen.GetTotal() });

                return totales;
            });
            task.ContinueWith(t =>
            {
                _totalesDelMesActual = t.Result;
                OnPropertyChanged("TotalesDelMesActual");
            });
            task.Start();
        }
    }
}
