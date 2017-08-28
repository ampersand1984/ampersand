using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class MovimientosViewModel : BaseViewModel, IMainWindowItem
    {
        public const string SIN_CATEGORIA = "Sin categoría";

        #region Constructor

        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoM, IMovimientosDataAccess movimientosDA)
        {
            _resumenAgrupadoM = resumenAgrupadoM;
            _movimientosDA = movimientosDA;
        }

        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoProyeccion, IMovimientosDataAccess movimientosDA, IEnumerable<BaseMovimiento> movimientosProyeccion)
            :this(resumenAgrupadoProyeccion, movimientosDA)
        {
            _esProyeccion = true;

            _movimientos = new ObservableCollection<BaseMovimiento>(movimientosProyeccion);
        }

        #endregion

        #region Fields

        private bool _esProyeccion;

        private IMovimientosDataAccess _movimientosDA;
        private ResumenAgrupadoModel _resumenAgrupadoM;

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

        private AgrupacionItem _totalesSelectedItem;
        public AgrupacionItem TotalesSelectedItem
        {
            get
            {
                return _totalesSelectedItem;
            }
            set
            {
                _totalesSelectedItem = value;
                if (_totalesSelectedItem == null)
                {
                    _movimientosFiltrados = null;
                }
                else
                {
                    _movimientosFiltrados = Movimientos.Where(a => a.TipoDescripcion.Contains(_totalesSelectedItem.Descripcion)).OrderBy(a => a.Fecha);
                }
                OnPropertyChanged("TotalesSelectedItem");
                OnPropertyChanged("MovimientosFiltrados");
            }
        }

        private ObservableCollection<BaseMovimiento> _movimientos;
        public ObservableCollection<BaseMovimiento> Movimientos
        {
            get
            {
                if (_movimientos == null)
                    _movimientos = GetMovimientos();
                return _movimientos;
            }
        }

        private IEnumerable<BaseMovimiento> _movimientosFiltrados;
        public IEnumerable<BaseMovimiento> MovimientosFiltrados
        {
            get
            {
                return _movimientosFiltrados ?? Movimientos;
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
            get { return _agrupaciones ?? (_agrupaciones = GetAgrupaciones(MovimientosFiltrados)); }
        }

        private AgrupacionItem _agrupacionesSelectedItem;
        public AgrupacionItem AgrupacionesSelectedItem
        {
            get
            {
                return _agrupacionesSelectedItem;
            }
            set
            {
                _agrupacionesSelectedItem = value;
                if (_agrupacionesSelectedItem == null)
                {
                    _movimientosFiltrados = null;
                }
                else
                {
                    if (_agrupacionesSelectedItem.Descripcion.Equals(SIN_CATEGORIA))
                        _movimientosFiltrados = Movimientos.Where(a => !a.Tags.Any()).OrderBy(a => a.Fecha);
                    else
                        _movimientosFiltrados = Movimientos.Where(a => a.Tags.Contains(_agrupacionesSelectedItem.Descripcion)).OrderBy(a => a.Fecha);
                }
                OnPropertyChanged("AgrupacionesSelectedItem");
                OnPropertyChanged("MovimientosFiltrados");
            }
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

        private BaseViewModel _editVM;
        public BaseViewModel EditVM
        {
            get { return _editVM; }
            set { _editVM = value; OnPropertyChanged("EditVM"); }
        }

        private BaseViewModel _modalVM;
        public BaseViewModel ModalVM
        {
            get { return _modalVM; }
            set { _modalVM = value; OnPropertyChanged("ModalVM"); }
        }

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
            get { return _editarMovimientoCommand ?? (_editarMovimientoCommand = new RelayCommand(param => this.EditarMovimientoCommandExecute(), param => EditarMovimientoCommandCanExecute())); }
        }

        private ICommand _importarInfoDeResumenAnteriorCommand;
        public ICommand ImportarInfoDeResumenAnteriorCommand
        {
            get
            {
                if (_importarInfoDeResumenAnteriorCommand == null)
                    _importarInfoDeResumenAnteriorCommand = new RelayCommand(param => ImportarInfoDeResumenAnteriorCommandExecute(), param => ImportarInfoDeResumenAnteriorCommandCanExecute());
                return _importarInfoDeResumenAnteriorCommand;
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

        #endregion

        #region Methods

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
                    var descri = string.Empty;
                    switch (resumen.Descripcion)
                    {
                        case "Visa":
                            descri = "1";
                            break;
                        case "Master Card":
                            descri = "2";
                            break;
                        default:
                            break;
                    }

                    var fileName = string.Format("r{0}{1}{2}", descri, resumen.FechaDeCierre.GetPeriodo(), resumen.FechaDeCierre.Day);
                    resumen.FilePath = Win32Helper.ShowFileDialog(fileName, string.Format("Nombre para el resúmen de {0}", resumen.Descripcion));
                    if (resumen.FilePath.IsNullOrEmpty())
                        return;

                    resumen.HuboCambios = true;
                }
            }

            _movimientosDA.SaveMovimientos(_resumenAgrupadoM, Movimientos);

            foreach (var resumen in _resumenAgrupadoM.Resumenes)
                resumen.HuboCambios = false;
        }        

        private bool ImportarInfoDeResumenAnteriorCommandCanExecute()
        {
            return EsElUtimoMes && _esProyeccion == false;
        }

        private void ImportarInfoDeResumenAnteriorCommandExecute()
        {
            //var movimientosAnteriores = _movimientosDA.GetMovimientosDeResumenAnterior(_resumenAgrupadoM.Periodo);

            //var movimientosActualizados = new List<BaseMovimiento>();

            //foreach (var movAnterior in movimientosAnteriores)
            //{
            //    var movActual = Movimientos.FirstOrDefault(a => a.IdMovimiento.Equals(movAnterior.IdMovimiento) &&
            //                                                    a.Descripcion.Equals(movAnterior.Descripcion));
            //    if (movActual != null)
            //    {
            //        movActual.Tags = movAnterior.Tags;
            //        movActual.DescripcionAdicional = movAnterior.DescripcionAdicional;
            //        movActual.EsMensual = movAnterior.EsMensual;
            //        movActual.EsAjeno = movAnterior.EsAjeno;

            //        movimientosActualizados.Add(movActual);
            //    }
            //    else
            //    {
            //        movActual = Movimientos.Except(movimientosActualizados)
            //                               .FirstOrDefault(a => a.Descripcion.Equals(movAnterior.Descripcion));
            //        if (movActual != null)
            //        {
            //            movActual.Tags = movAnterior.Tags;
            //            movActual.DescripcionAdicional = movAnterior.DescripcionAdicional;
            //            movActual.EsMensual = movAnterior.EsMensual;
            //            movActual.EsAjeno = movAnterior.EsAjeno;

            //            movimientosActualizados.Add(movActual);
            //        }
            //    }
            //}

            foreach (var resumen in _resumenAgrupadoM.Resumenes)
                resumen.HuboCambios = true;
        }

        protected override void OnRequestCloseEvent()
        {
            if (EditVM != null)
                EditVM.CloseCommand.Execute(null);
            else
            {
                if (HuboCambios)
                {
                    var messageBoxResult = MessageBox.Show("Cerrar sin gurdar cambios?", "Cerrar", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (messageBoxResult == MessageBoxResult.Yes)
                        base.OnRequestCloseEvent();
                }
                else
                    base.OnRequestCloseEvent();
            }
        }

        private bool EditarMovimientoCommandCanExecute()
        {
            return SelectedIndex != -1;
        }

        private void EditarMovimientoCommandExecute()
        {
            var baseMovimiento = Movimientos.ElementAt(SelectedIndex);
            var movimientoABMVM = new MovimientoABMViewModel(baseMovimiento);
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            EditVM = movimientoABMVM;
        }

        private void MovimientoABMVM_SaveEvent(object sender, MovimientoABMSaveEventArgs e)
        {
            var movimientoABMVM = sender as MovimientoABMViewModel;
            var resumen = _resumenAgrupadoM.Resumenes.First(a => a.Descripcion.Equals(movimientoABMVM.TipoDescripcion));
            resumen.HuboCambios = true;

            if (_movimientos.IndexOf(e.Model) == -1)
            {
                _movimientos.Add(e.Model);
                OnPropertyChanged("Movimientos");
                OnPropertyChanged("MovimientosFiltrados");
                SelectedIndex = _movimientos.IndexOf(_movimientos.Last());
            }
            _totales = null;
            _agrupaciones = null;

            OnPropertyChanged("Totales");
            OnPropertyChanged("Agrupaciones");
        }

        private void MovimientoABMVM_CloseEvent(object sender, EventArgs e)
        {
            var movimientoABMVM = sender as MovimientoABMViewModel;
            movimientoABMVM.CloseEvent -= MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent -= MovimientoABMVM_SaveEvent;
            EditVM = null;
        }

        private bool ProyectarCommandCanExecute()
        {
            return EsElUtimoMes;
        }

        private void ProyectarCommandExecute()
        {
            var movimientosProyeccion = new List<BaseMovimiento>();

            foreach (var mov in Movimientos.Where(a => a.CoutasPendientes > 0)
                                           .OrderByDescending(a => a.CoutasPendientes))
            {
                var movProy = mov.Clone() as BaseMovimiento;
                movProy.IncrementarCuotasPendientes();
                movimientosProyeccion.Add(movProy);
            }

            foreach (var mov in Movimientos.Where(a => a.EsMensual))
            {
                var movProy = mov.Clone() as BaseMovimiento;
                movProy.Fecha = movProy.Fecha.AddMonths(1);
                movimientosProyeccion.Add(movProy);
            }

            var resumenAgrupadoProyeccion = _resumenAgrupadoM.Clone() as ResumenAgrupadoModel;

            foreach (var resumen in resumenAgrupadoProyeccion.Resumenes)
            {
                resumen.FechaDeCierre = resumen.ProximoCierre != DateTime.MinValue ? 
                    resumen.ProximoCierre : 
                    resumen.FechaDeCierre.AddMonths(1);

                resumen.ProximoCierre = DateTime.MinValue;
                resumen.FilePath = string.Empty;
                resumen.Total = movimientosProyeccion.Where(a => a.TipoDescripcion.Equals(resumen.Descripcion)).Sum(a => a.Monto);
            }
            var movimientosVM = new MovimientosViewModel(resumenAgrupadoProyeccion, _movimientosDA, movimientosProyeccion);
            OnPublishViewModelEvent(movimientosVM);
        }

        private bool NuevoMovimientoCommandCanExecute()
        {
            return EsElUtimoMes;
        }

        private void NuevoMovimientoCommandExecute()
        {
            var movimientoABMVM = new MovimientoABMViewModel();
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            EditVM = movimientoABMVM;
        }

        private ObservableCollection<BaseMovimiento> GetMovimientos()
        {
            var movimientos = _movimientosDA.GetMovimientos(_resumenAgrupadoM);

            movimientos = new ObservableCollection<BaseMovimiento>(movimientos);

            _agrupaciones = GetAgrupaciones(movimientos);

            OnPropertyChanged("TotalResumen");

            return new ObservableCollection<BaseMovimiento>(movimientos);
        }

        private IEnumerable<AgrupacionItem> GetTotales()
        {
            var totales = new List<AgrupacionItem>();

            foreach (var item in _resumenAgrupadoM.Resumenes)
                totales.Add(new AgrupacionItem() { Descripcion = item.Descripcion, Monto = item.Total });

            return totales;
        }

        private IEnumerable<AgrupacionItem> GetAgrupaciones(IEnumerable<BaseMovimiento> movimientos)
        {
            var tags = new List<AgrupacionItem>();
            foreach (var mov in movimientos.Where(a => a.Tags.Any()).OrderBy(b => b.Monto))
            {
                var tagItem = tags.FirstOrDefault(a => a.Descripcion == mov.Tags.First());
                if (tagItem != null)
                    tagItem.Monto += mov.Monto;
                else
                    tags.Add(new AgrupacionItem() { Descripcion = mov.Tags.First(), Monto = mov.Monto });
            }

            var sinTags = movimientos.Where(a => !a.Tags.Any()).Sum(b => b.Monto);
            if (sinTags > 0.00M)
                tags.Add(new AgrupacionItem() { Descripcion = SIN_CATEGORIA, Monto = sinTags });

            return tags.OrderByDescending(a => a.Descripcion);
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
    }
}
