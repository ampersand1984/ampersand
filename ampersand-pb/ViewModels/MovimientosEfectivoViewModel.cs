using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ampersand_pb.ViewModels
{
    public class MovimientosEfectivoViewModel : BaseViewModel, IMainWindowItem
    {
        #region Constructor 

        public MovimientosEfectivoViewModel(ConfiguracionModel configuracionM, IMovimientosDataAccess movimientosDA)
        {
            _configuracionM = configuracionM;
            _movimientosDA = movimientosDA;

            var fecha = DateTime.Today;

            DisplayName = "Efectivo " + fecha.ToString("MMM yyyy");

            _resumenActual = _movimientosDA.GetResumen(fecha.GetPeriodo());

            var periodoAnterior = _resumenActual.Periodo.GetPeriodoAnterior();

            _resumenAnterior = _movimientosDA.GetResumen(periodoAnterior);

            var x = MovimientosActual;
        }

        #endregion

        #region Fields

        private readonly ConfiguracionModel _configuracionM;
        private readonly IMovimientosDataAccess _movimientosDA;

        private ResumenAgrupadoModel _resumenActual;
        private readonly ResumenAgrupadoModel _resumenAnterior;

        #endregion

        #region Properties

        public string DisplayName { get; }

        public IDialogCoordinator DialogCoordinator { get; set; }


        private ObservableCollection<BaseMovimiento> _movimientosActual;

        public ObservableCollection<BaseMovimiento> MovimientosActual
        {
            get
            {
                if (_movimientosActual == null)
                {
                    _movimientosActual = GetMovimientosActual();
                }
                return _movimientosActual;
            }
        }

        public decimal TotalActual
        {
            get
            {
                var total = MovimientosActual.Sum(a => a.Monto);

                return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
            }
        }

        #endregion

        #region Methods

        private ObservableCollection<BaseMovimiento> GetMovimientosActual()
        {
            var movimientos = _movimientosDA.GetMovimientos(_resumenActual.Resumenes.First(a => a.Tipo == TiposDeMovimiento.Efectivo))
                .ToList();

            //agrego las tarjetas del mes pasado, y los ajenos
            var tarjetasPeriodoAnterior = _resumenAnterior.Resumenes.Where(a => a.Tipo == TiposDeMovimiento.Credito);

            foreach (var resumenTarjeta in tarjetasPeriodoAnterior)
            {
                var fecha = (resumenTarjeta.Periodo + "01").ToDateTime().ToString("MMMM yyyy");

                var pagoDeTarjeta = new GastoModel
                {
                    Descripcion = $"{resumenTarjeta.Descripcion} {fecha}",
                    Tipo = TiposDeMovimiento.Efectivo
                };

                var total = resumenTarjeta.GetTotal();
                total = decimal.Round(total, 2, MidpointRounding.AwayFromZero);

                pagoDeTarjeta.SetMonto(total * -1);

                movimientos.Add(pagoDeTarjeta);

                var movimientosTarjeta = _movimientosDA.GetMovimientos(resumenTarjeta);

                var ajenos = movimientosTarjeta.Where(a => a.EsAjeno);

                foreach (var movAjeno in ajenos)
                {
                    movAjeno.SetMonto(movAjeno.Monto);
                    movimientos.Add(movAjeno);
                }
            }

            //agregar los ajenos? 
            //marcarlos para sumarlos o no?

            movimientos = movimientos.Where(a => a.Monto > 0)
                                     .OrderByDescending(a => a.Monto)
                                     .Union(movimientos.Where(a => a.Monto <= 0)
                                                       .OrderBy(a => a.Monto)).ToList();

            movimientos.ForEach(a => a.RefrescarMontosTerminado += ItemMovimientoActual_RefrescarMontosTerminado);

            return new ObservableCollection<BaseMovimiento>(movimientos);
        }

        private void ItemMovimientoActual_RefrescarMontosTerminado(object sender, EventArgs e)
        {
            OnPropertyChanged("TotalActual");
        }

        #endregion

        #region Events

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            this.PublishViewModelEvent?.Invoke(this, new PublishViewModelEventArgs(viewModel));
        }

        #endregion
    }
}
