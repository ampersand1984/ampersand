using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using ampersand_pb.Properties;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class MovimientosViewModel : BaseViewModel, IMainWindowItem
    {
        public enum TiposDeAgrupacion  { MedioDePago, Tag, Totales }

        #region Constructor

        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoM, IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            _resumenAgrupadoM = resumenAgrupadoM;
            _movimientosDA = movimientosDA;
            _configuracionM = configuracionM;

            if (!_esProyeccion)
                _movimientos = GetMovimientos().ToList();
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                if (_seleccionDeMediosDePagoVM != null)
                {
                    _seleccionDeMediosDePagoVM.SeleccionDeMediosDePagoCambiada -= SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada;
                    _seleccionDeMediosDePagoVM.Dispose();
                }
                _seleccionDeMediosDePagoVM = null;
            }
            base.Dispose(dispose);
        }

        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoProyeccion, IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM, IEnumerable<BaseMovimiento> movimientosProyeccion)
            : this(resumenAgrupadoProyeccion, movimientosDA, configuracionM)
        {
            _esProyeccion = true;

            _movimientos = new List<BaseMovimiento>(movimientosProyeccion);
        }

        #endregion

        #region Fields

        private readonly bool _esProyeccion;

        private readonly ResumenAgrupadoModel _resumenAgrupadoM;
        private readonly IMovimientosDataAccess _movimientosDA;
        private readonly ConfiguracionModel _configuracionM;

        private List<BaseMovimiento> _movimientos;

        #endregion

        #region Properties

        public string DisplayName
        {
            get
            {
                return TextoPeriodo;
            }
        }

        public string TextoPeriodo
        {
            get
            {
                return _resumenAgrupadoM.TextoPeriodo;
            }
        }

        public decimal TotalResumen
        {
            get { return MovimientosFiltrados.Sum(a => a.Monto); }
        }

        private IEnumerable<AgrupacionItem> _totales;
        public IEnumerable<AgrupacionItem> Totales
        {
            get
            {
                return _totales ?? (_totales = GetTotales());
            }
        }

        private AgrupacionItem _graficosSelectedItem;
        public AgrupacionItem GraficosSelectedItem
        {
            get
            {
                return _graficosSelectedItem;
            }
            set
            {
                _graficosSelectedItem = value;
                _movimientosFiltrados = null;
                OnPropertyChanged("GraficosSelectedItem");
                OnPropertyChanged("MovimientosFiltrados");
                OnPropertyChanged("PermiteLimpiarSeleccion");
            }
        }

        public bool PermiteLimpiarSeleccion
        {
            get
            {
                return GraficosSelectedItem != null;
            }
        }

        private ObservableCollection<BaseMovimiento> _movimientosFiltrados;
        public ObservableCollection<BaseMovimiento> MovimientosFiltrados
        {
            get
            {
                return _movimientosFiltrados ?? (_movimientosFiltrados = new ObservableCollection<BaseMovimiento>(GetMovimientosFiltrados()));
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; OnPropertyChanged("SelectedIndex"); }
        }

        private IEnumerable<AgrupacionItem> _agrupaciones;
        public IEnumerable<AgrupacionItem> Agrupaciones
        {
            get { return _agrupaciones ?? (_agrupaciones = GetAgrupaciones()); }
        }

        public bool EsElUtimoMes
        {
            get
            {
                return _resumenAgrupadoM.EsElUtimoMes;
            }
        }

        public bool HuboCambios
        {
            get
            {
                return _resumenAgrupadoM.Resumenes.Any(a => a.HuboCambios);
            }
        }

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

        public IDialogCoordinator DialogCoordinator { get; set; }

        private ICommand _proyectarCommand;
        public ICommand ProyectarCommand
        {
            get
            {
                if (_proyectarCommand == null)
                    _proyectarCommand = new RelayCommand(param => ProyectarCommandExecute(), param => ProyectarCommandCanExecute());
                return _proyectarCommand;
            }
        }

        private ICommand _verMesAnteriorCommand;
        public ICommand VerMesAnteriorCommand
        {
            get
            {
                if (_verMesAnteriorCommand == null)
                    _verMesAnteriorCommand = new RelayCommand(param => VerMesAnteriorCommandExecute());
                return _verMesAnteriorCommand;
            }
        }

        private ICommand _nuevoMovimientoCommand;
        public ICommand NuevoMovimientoCommand
        {
            get
            {
                if (_nuevoMovimientoCommand == null)
                    _nuevoMovimientoCommand = new RelayCommand(param => NuevoMovimientoCommandExecute(), param => NuevoMovimientoCommandCanExecute());
                return _nuevoMovimientoCommand;
            }
        }

        private ICommand _editarMovimientoCommand;
        public ICommand EditarMovimientoCommand
        {
            get { return _editarMovimientoCommand ?? (_editarMovimientoCommand = new RelayCommand(param => this.EditarMovimientoCommandExecute(param as BaseMovimiento), param => EditarMovimientoCommandCanExecute(param as BaseMovimiento))); }
        }

        private ICommand _eliminarSeleccionadoCommand;
        public ICommand EliminarSeleccionadoCommand
        {
            get
            {
                if (_eliminarSeleccionadoCommand == null)
                    _eliminarSeleccionadoCommand = new RelayCommand(param => EliminarSeleccionadoCommandExecute(param as BaseMovimiento), param => EliminarSeleccionadoCommandCanExecute(param as BaseMovimiento));

                return _eliminarSeleccionadoCommand;
            }
        }

        private ICommand _copiarSeleccionadoCommand;
        public ICommand CopiarSeleccionadoCommand
        {
            get
            {
                if (_copiarSeleccionadoCommand == null)
                    _copiarSeleccionadoCommand = new RelayCommand(param => CopiarSeleccionadoCommandExecute(param as BaseMovimiento), param => CopiarSeleccionadoCommandCanExecute(param as BaseMovimiento));

                return _copiarSeleccionadoCommand;
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(param => SaveCommandExecute(), param => SaveCommandCanExecute());
                return _saveCommand;
            }
        }

        private ICommand _limpiarSeleccionCommand;
        public ICommand LimpiarSeleccionCommand
        {
            get
            {
                if (_limpiarSeleccionCommand == null)
                    _limpiarSeleccionCommand = new RelayCommand(param => RefrescarMovimientos());
                return _limpiarSeleccionCommand;
            }
        }

        private ICommand _buscarCommand;
        public ICommand BuscarCommand
        {
            get
            {
                if (_buscarCommand == null)
                    _buscarCommand = new RelayCommand(param => BuscarCommandExecute(param as string));

                return _buscarCommand;
            }
        }

        #endregion

        #region Methods

        private async Task<MessageDialogResult> ShowMessage(MessageParam messageParam)
        {
            var result = await DialogCoordinator.ShowMessageAsync(this, messageParam.Title, messageParam.Message, messageParam.Style, messageParam.Settings);

            return result;
        }

        private bool SaveCommandCanExecute()
        {
            return _esProyeccion || HuboCambios;
        }

        private void SaveCommandExecute()
        {
            if (_esProyeccion)
            {
                foreach (var resumen in _resumenAgrupadoM.Resumenes.Where(a => a.FilePath.IsNullOrEmpty()))
                {
                    var fileName = string.Format("{0}{1}", resumen.Id, resumen.FechaDeCierre.GetPeriodo());
                    resumen.FilePath = Win32Helper.ShowFileDialog(fileName, string.Format("Nombre para el resúmen de {0}", resumen.Descripcion));
                    if (resumen.FilePath.IsNullOrEmpty())
                        return;

                    resumen.HuboCambios = true;
                }
            }

            _movimientosDA.SaveMovimientos(_resumenAgrupadoM, _movimientos);

            foreach (var resumen in _resumenAgrupadoM.Resumenes)
                resumen.HuboCambios = false;
        }

        private void CopiarSeleccionadoCommandExecute(BaseMovimiento param)
        {
            var copia = param.Clone() as BaseMovimiento;
            copia.IdMovimiento = 0;

            if (copia.EsMonedaExtranjera)
                copia.SetMontoME(0, copia.Cotizacion);
            else
                copia.SetMonto(0);

            copia.Fecha = DateTime.Today;
            var movimientoABMVM = new MovimientoABMViewModel(copia, _configuracionM, esCopia: true);
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            OnPublishViewModelEvent(movimientoABMVM);
        }

        private bool CopiarSeleccionadoCommandCanExecute(BaseMovimiento param)
        {
            return param != null;
        }

        private async void EliminarSeleccionadoCommandExecute(BaseMovimiento param)
        {
            var strMessage = string.Format("Eliminar {0}?", param);

            var messageParam = new MessageParam("Confirmar eliminación", strMessage, MessageDialogStyle.AffirmativeAndNegative, null);

            var result = await ShowMessage(messageParam);
            if (result == MessageDialogResult.Affirmative)
            {
                var resumen = _resumenAgrupadoM.Resumenes.First(a => a.Id == param.IdResumen);
                resumen.HuboCambios = true;

                _movimientos.Remove(param);

                RefrescarMovimientos();
            }
        }

        private bool EliminarSeleccionadoCommandCanExecute(BaseMovimiento param)
        {
            return param != null;
        }

        private void BuscarCommandExecute(string texto)
        {
            _graficosSelectedItem = null;

            var resultado = GetMovimientosFiltrados();
            resultado = resultado.Where(a => a.Descripcion.ToLower().Contains(texto.ToLower()) ||
                                             a.DescripcionAdicional.ToLower().Contains(texto.ToLower()) ||
                                             a.Tags.Any(t => t.ToLower().Contains(texto.ToLower())));
            
            _totales = null;
            _agrupaciones = null;
            _movimientosFiltrados = new ObservableCollection<BaseMovimiento>(resultado);

            OnPropertyChanged("GraficosSelectedItem");
            OnPropertyChanged("MovimientosFiltrados");
            OnPropertyChanged("TotalResumen");
            OnPropertyChanged("PermiteLimpiarSeleccion");
        }

        protected override void OnRequestCloseEvent()
        {
            if (HuboCambios)
            {
                var messageBoxResult = MessageBox.Show("Cerrar sin gurdar cambios?", "Cerrar", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (messageBoxResult == MessageBoxResult.Yes)
                    base.OnRequestCloseEvent();
                //var param = new MessageParam(DisplayName, "Cerrar sin gurdar cambios?", MessageDialogStyle.AffirmativeAndNegative, null);

                //var result = await ShowMessage(param);
                //if (result == MessageDialogResult.Affirmative)
                //    base.OnRequestCloseEvent();
            }
            else
                base.OnRequestCloseEvent();
        }

        private bool EditarMovimientoCommandCanExecute(BaseMovimiento param)
        {
            return param != null;
        }

        private void EditarMovimientoCommandExecute(BaseMovimiento param)
        {
            var movimientoABMVM = new MovimientoABMViewModel(param, _configuracionM);
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            OnPublishViewModelEvent(movimientoABMVM);
        }

        private void MovimientoABMVM_SaveEvent(object sender, MovimientoABMSaveEventArgs e)
        {
            if (_movimientos.IndexOf(e.Model) == -1)
            {
                _movimientos.Add(e.Model);
                OnPropertyChanged("Movimientos");
                OnPropertyChanged("MovimientosFiltrados");
                SelectedIndex = MovimientosFiltrados.IndexOf(e.Model);
            }

            var resumen = _resumenAgrupadoM.Resumenes.FirstOrDefault(a => a.Id.Equals(e.Model.IdResumen));
            if (resumen != null)
                resumen.HuboCambios = true;

            var resumenOriginal = _resumenAgrupadoM.Resumenes.FirstOrDefault(a => a.Id.Equals(e.IdResumenOriginal));
            if (resumenOriginal != null)
                resumenOriginal.HuboCambios = true;

            RefrescarMovimientos();
        }

        private void RefrescarMovimientos()
        {
            _graficosSelectedItem = null;
            _totales = null;
            _agrupaciones = null;
            _movimientosFiltrados = null;

            OnPropertyChanged("GraficosSelectedItem");
            OnPropertyChanged("Totales");
            OnPropertyChanged("Agrupaciones");
            OnPropertyChanged("MovimientosFiltrados");
            OnPropertyChanged("TotalResumen");
            OnPropertyChanged("PermiteLimpiarSeleccion");
        }

        private IEnumerable<BaseMovimiento> GetMovimientosFiltrados()
        {
            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            var movimientosFiltrados = _movimientos.Where(a => mediosDePago.Contains(a.IdResumen));

            if (GraficosSelectedItem != null)
            {
                switch (GraficosSelectedItem.Tipo)
                {
                    case TiposDeAgrupacion.Totales:
                    case TiposDeAgrupacion.MedioDePago:
                        {
                            movimientosFiltrados = movimientosFiltrados.Where(a => a.IdResumen.Equals(_graficosSelectedItem.Id));
                        }
                        break;
                    case TiposDeAgrupacion.Tag:
                        {
                            if (GraficosSelectedItem.Descripcion.Equals(TagModel.SIN_CATEGORIA))
                                movimientosFiltrados = movimientosFiltrados.Where(a => !a.Tags.Any());
                            else
                                movimientosFiltrados = movimientosFiltrados.Where(a => a.Tags.Contains(GraficosSelectedItem.Descripcion));
                        }
                        break;
                    default:
                        break;
                }
            }

            return movimientosFiltrados.OrderBy(a => !a.EsMensual).ThenBy(a => a.Fecha);
        }

        private void MovimientoABMVM_CloseEvent(object sender, EventArgs e)
        {
            var movimientoABMVM = sender as MovimientoABMViewModel;
            movimientoABMVM.CloseEvent -= MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent -= MovimientoABMVM_SaveEvent;
            movimientoABMVM = null;
        }

        private void VerMesAnteriorCommandExecute()
        {
            var year = _resumenAgrupadoM.Periodo.Substring(0, 4);
            var month = _resumenAgrupadoM.Periodo.Substring(4, 2);
            var periodo = new DateTime(int.Parse(year), int.Parse(month), 1).AddMonths(-1).ToString("yyyyMM");
            var resumenAgrupado = _movimientosDA.GetResumen(periodo);
                
            var movimientosVM = new MovimientosViewModel(resumenAgrupado, _movimientosDA, _configuracionM);
            OnPublishViewModelEvent(movimientosVM);
        }

        private bool ProyectarCommandCanExecute()
        {
            return EsElUtimoMes;
        }

        private void ProyectarCommandExecute()
        {
            var movimientosProyeccion = GetProyeccion(_movimientos);

            var resumenAgrupadoProyeccion = _resumenAgrupadoM.Clone() as ResumenAgrupadoModel;

            foreach (var resumen in resumenAgrupadoProyeccion.Resumenes)
            {
                resumen.FechaDeCierre = resumen.ProximoCierre.Month == resumen.FechaDeCierre.Month + 1 ?
                    resumen.ProximoCierre :
                    resumen.FechaDeCierre.AddMonths(1);

                resumen.Periodo = resumen.FechaDeCierre.GetPeriodo();
                resumen.ProximoCierre = DateTime.MinValue;
                resumen.FilePath = string.Empty;
                resumen.Total = movimientosProyeccion.Where(a => a.DescripcionResumen.Equals(resumen.Descripcion)).Sum(a => a.Monto);
            }
            var movimientosVM = new MovimientosViewModel(resumenAgrupadoProyeccion, _movimientosDA, _configuracionM, movimientosProyeccion);
            OnPublishViewModelEvent(movimientosVM);
        }

        public static IEnumerable<BaseMovimiento> GetProyeccion(IEnumerable<BaseMovimiento> movimientos)
        {
            var movimientosProyeccion = new List<BaseMovimiento>();

            foreach (var mov in movimientos.Where(a => a.CoutasPendientes > 0)
                                           .OrderByDescending(a => a.CoutasPendientes))
            {
                var movProy = mov.Clone() as BaseMovimiento;
                movProy.IncrementarCuotasPendientes();
                movimientosProyeccion.Add(movProy);
            }

            foreach (var mov in movimientos.Where(a => a.EsMensual))
            {
                var movProy = mov.Clone() as BaseMovimiento;
                movProy.Fecha = movProy.Fecha.AddMonths(1);
                movimientosProyeccion.Add(movProy);
            }

            return movimientosProyeccion;
        }

        private bool NuevoMovimientoCommandCanExecute()
        {
            return EsElUtimoMes;
        }

        private void NuevoMovimientoCommandExecute()
        {
            var movimientoABMVM = new MovimientoABMViewModel(_configuracionM);
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            OnPublishViewModelEvent(movimientoABMVM);
        }

        private IEnumerable<BaseMovimiento> GetMovimientos()
        {
            var movimientos = _movimientosDA.GetMovimientos(_resumenAgrupadoM);

            return new List<BaseMovimiento>(movimientos);
        }

        private IEnumerable<AgrupacionItem> GetTotales()
        {
            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            var totales = new List<AgrupacionItem>();

            foreach (var item in _resumenAgrupadoM.Resumenes.Where(a => mediosDePago.Contains(a.Id)))
                totales.Add(new AgrupacionItem() { Tipo = TiposDeAgrupacion.MedioDePago, Id = item.Id, Descripcion = item.Descripcion, Monto = item.Total });

            return totales;
        }

        private IEnumerable<AgrupacionItem> GetAgrupaciones()
        {
            var mediosDePago = SeleccionDeMediosDePagoVM.GetIds();

            var tags = new List<AgrupacionItem>();
            foreach (var mov in _movimientos.Where(a => mediosDePago.Contains(a.IdResumen)).Where(a => a.Tags.Any()).OrderBy(b => b.Monto))
            {
                var tagItem = tags.FirstOrDefault(a => a.Descripcion == mov.Tags.First());
                if (tagItem != null)
                    tagItem.Monto += mov.Monto;
                else
                    tags.Add(new AgrupacionItem() { Tipo = TiposDeAgrupacion.Tag, Descripcion = mov.Tags.First(), Monto = mov.Monto });
            }

            var sinTags = _movimientos.Where(a => mediosDePago.Contains(a.IdResumen)).Where(a => !a.Tags.Any()).Sum(b => b.Monto);
            if (sinTags > 0.00M)
                tags.Add(new AgrupacionItem() { Tipo = TiposDeAgrupacion.Tag, Descripcion = TagModel.SIN_CATEGORIA, Monto = sinTags });

            return tags.OrderByDescending(a => a.Descripcion);
        }

        private void SeleccionDeMediosDePagoVM_SeleccionDeMediosDePagoCambiada(object sender, EventArgs e)
        {
            RefrescarMovimientos();
        }

        #endregion

        #region Events

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            var handler = this.PublishViewModelEvent;
            if (handler != null)
                handler(this, new PublishViewModelEventArgs(viewModel));
        }

        #endregion
    }

    public class AgrupacionItem
    {
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public MovimientosViewModel.TiposDeAgrupacion Tipo { get; internal set; }

        private string _id;
        public string Id
        {
            get { return _id ?? string.Empty; }
            set { _id = value; }
        }
    }
}
