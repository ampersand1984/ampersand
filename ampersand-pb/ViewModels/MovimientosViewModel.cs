using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class MovimientosViewModel : BaseViewModel, IMainWindowItem
    {
        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoM, IMovimientosDataAccess movimientosDA)
        {
            _resumenAgrupadoM = resumenAgrupadoM;
            _movimientosDA = movimientosDA;
        }

        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoProyeccion, IMovimientosDataAccess movimientosDA, IEnumerable<BaseMovimiento> movimientosProyeccion)
            :this(resumenAgrupadoProyeccion, movimientosDA)
        {
            _esProyeccion = true;

            _movimientos = movimientosProyeccion;
        }

        private bool _esProyeccion;

        private IMovimientosDataAccess _movimientosDA;
        private ResumenAgrupadoModel _resumenAgrupadoM;

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
            get { return  Movimientos.Sum(a => a.Monto); }
        }

        private bool _filtrar;
        public bool Filtrar
        {
            get
            {
                return _filtrar;
            }
            set
            {
                _filtrar = value;
                OnPropertyChanged("Filtrar");
            }
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
                OnPropertyChanged("TotalesSelectedItem");
            }
        }

        private IEnumerable<BaseMovimiento> _movimientos;
        public IEnumerable<BaseMovimiento> Movimientos
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
                OnPropertyChanged("AgrupacionesSelectedItem");
            }
        }

        public bool EsElUtimoMes
        {
            get
            {
                return _resumenAgrupadoM.EsElUtimoMes;
            }
        }

        private MovimientoABMViewModel _editVM;
        public MovimientoABMViewModel EditVM
        {
            get { return _editVM; }
            set { _editVM = value; OnPropertyChanged("EditVM"); }
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

        private bool SaveCommandCanExecute()
        {
            return _esProyeccion == false && _resumenAgrupadoM.Resumenes.Any(a => a.HuboCambios);
        }

        private void SaveCommandExecute()
        {
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
                base.OnRequestCloseEvent();
        }

        private bool EditarMovimientoCommandCanExecute()
        {
            return _esProyeccion == false && SelectedIndex != -1;
        }

        private void EditarMovimientoCommandExecute()
        {
            var tags = Movimientos.SelectMany(a => a.Tags).Distinct().GetTags();

            var baseMovimiento = Movimientos.ElementAt(SelectedIndex);
            var movimientoABMVM = new MovimientoABMViewModel(baseMovimiento, tags);
            movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
            EditVM = movimientoABMVM;
        }

        private void MovimientoABMVM_SaveEvent(object sender, EventArgs e)
        {
            var movimientoABMVM = sender as MovimientoABMViewModel;
            var resumen = _resumenAgrupadoM.Resumenes.First(a => a.Descripcion.Equals(movimientoABMVM.TipoDescripcion));
            resumen.HuboCambios = true;

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
                resumen.FechaDeCierre = resumen.FechaDeCierre.AddMonths(1);
                resumen.Total = movimientosProyeccion.Where(a => a.TipoDescripcion.Equals(resumen.Descripcion)).Sum(a => a.Monto);
            }
            var movimientosVM = new MovimientosViewModel(resumenAgrupadoProyeccion, _movimientosDA, movimientosProyeccion);
            OnPublishViewModelEvent(movimientosVM);
        }

        private IEnumerable<BaseMovimiento> GetMovimientos()
        {
            var movimientos = _movimientosDA.GetMovimientos(_resumenAgrupadoM);

            movimientos = new ObservableCollection<BaseMovimiento>(movimientos);

            _agrupaciones = GetAgrupaciones(movimientos);

            OnPropertyChanged("TotalResumen");

            return movimientos;
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
            
            return tags;
        }

        private string _ultimaOpcionDeGraficoSeleccionada = "TotalesSelectedItem";

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case "TotalesSelectedItem":
                    {
                        if (TotalesSelectedItem != null)
                        {
                            var seleccion = TotalesSelectedItem.Descripcion;

                            foreach (var mov in Movimientos)
                                mov.IsSelected = mov.TipoDescripcion.Equals(seleccion);
                        }
                        else
                        {
                            foreach (var mov in Movimientos)
                                mov.IsSelected = false;
                        }
                        _ultimaOpcionDeGraficoSeleccionada = propertyName;
                        ActualizarMovimientosFiltrados();
                    }
                    break;

                case "AgrupacionesSelectedItem":
                    {
                        if (AgrupacionesSelectedItem != null)
                        {
                            var seleccion = AgrupacionesSelectedItem.Descripcion;

                            foreach (var mov in Movimientos)
                                mov.IsSelected = mov.Tags.Any(a => a.Equals(seleccion));
                        }
                        else
                        {
                            foreach (var mov in Movimientos)
                                mov.IsSelected = false;
                        }
                        _ultimaOpcionDeGraficoSeleccionada = propertyName;
                        ActualizarMovimientosFiltrados();
                    }
                    break;

                case "Filtrar":
                    ActualizarMovimientosFiltrados();
                    break;

                default:
                    break;
            }
        }

        private void ActualizarMovimientosFiltrados()
        {
            if (Filtrar)
            {
                switch (_ultimaOpcionDeGraficoSeleccionada)
                {
                    case "TotalesSelectedItem":
                        if (TotalesSelectedItem != null)
                            _movimientosFiltrados = Movimientos.Where(a => a.TipoDescripcion.Equals(TotalesSelectedItem.Descripcion));
                        break;

                    case "AgrupacionesSelectedItem":
                        if (AgrupacionesSelectedItem != null)
                            _movimientosFiltrados = Movimientos.Where(a => a.Tags.Any(b => b.Equals(AgrupacionesSelectedItem.Descripcion)));
                        break;

                    default:
                        break;
                }
            }
            else
            {
                _movimientosFiltrados = null;
            }
            OnPropertyChanged("MovimientosFiltrados");
        }

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            var handler = this.PublishViewModelEvent;
            if (handler != null)
                handler(this, new PublishViewModelEventArgs(viewModel));
        }
    }

    public class AgrupacionItem
    {
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
    }
}
