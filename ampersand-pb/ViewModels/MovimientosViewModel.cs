using System;
using System.Linq;
using ampersand_pb.DataAccess;
using ampersand.Core;
using ampersand.Core.Common;
using System.Windows.Input;
using ampersand_pb.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace ampersand_pb.ViewModels
{
    public class MovimientosViewModel : BaseViewModel, IMainWindowItem
    {
        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoM, IMovimientosDataAccess movimientosDA)
        {
            _resumenAgrupadoM = resumenAgrupadoM;
            _movimientosDA = movimientosDA;
        }

        public MovimientosViewModel(ResumenAgrupadoModel resumenAgrupadoM, IMovimientosDataAccess movimientosDA, IEnumerable<BaseMovimiento> movimientos)
            :this(resumenAgrupadoM, movimientosDA)
        {
            _esProyeccion = true;

            _movimientos = new ObservableCollection<BaseMovimiento>(movimientos);

            _agrupaciones = GetAgrupaciones(_movimientos);
        }

        private bool _esProyeccion;
        private bool _huboCambios;

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

        public decimal TotalSeleccion
        {
            get { return Movimientos.Where(a => a.IsSelected).Sum(a => a.Monto); }
        }        

        private ObservableCollection<BaseMovimiento> _movimientos;
        public ObservableCollection<BaseMovimiento> Movimientos
        {
            get
            {
                if (_movimientos == null)
                    CargarMovimientos();
                return _movimientos;
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
            get { return _agrupaciones; }
        }

        private IEnumerable<AgrupacionItem> _agrupacionesSeleccion;
        public IEnumerable<AgrupacionItem> AgrupacionesSeleccion
        {
            get { return _agrupacionesSeleccion; }
            set { _agrupacionesSeleccion = value; OnPropertyChanged("AgrupacionesSeleccion"); }
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
            return _huboCambios && _esProyeccion == false;
        }

        private void SaveCommandExecute()
        {
            //_movimientosDA.SaveMovimientos(_resumenAgrupadoM, Movimientos);
            //_huboCambios = false;
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

            //_huboCambios = true;
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
            return _esProyeccion == false;
        }

        private void EditarMovimientoCommandExecute()
        {
            if (SelectedIndex != -1)
            {
                var tags = Movimientos.SelectMany(a => a.Tags).Distinct().GetTags();

                var baseMovimiento = Movimientos.ElementAt(SelectedIndex);
                var movimientoABMVM = new MovimientoABMViewModel(baseMovimiento, tags);
                movimientoABMVM.CloseEvent += MovimientoABMVM_CloseEvent;
                movimientoABMVM.SaveEvent += MovimientoABMVM_SaveEvent;
                EditVM = movimientoABMVM;
            }
        }

        private void MovimientoABMVM_SaveEvent(object sender, EventArgs e)
        {
            var movimientoABMVM = sender as MovimientoABMViewModel;
            movimientoABMVM.CloseEvent -= MovimientoABMVM_CloseEvent;
            movimientoABMVM.SaveEvent -= MovimientoABMVM_SaveEvent;
            movimientoABMVM = null;
        }

        private void MovimientoABMVM_CloseEvent(object sender, EventArgs e)
        {
            EditVM = null;
            _huboCambios = true;
        }

        private bool ProyectarCommandCanExecute()
        {
            return EsElUtimoMes;
        }

        private void ProyectarCommandExecute()
        {
            //var movimientos = new List<BaseMovimiento>();

            //foreach (var mov in Movimientos.Where(a => a.CoutasPendientes > 0)
            //                               .OrderByDescending(a => a.CoutasPendientes))
            //{
            //    var movProy = mov.Clone() as BaseMovimiento;
            //    movProy.IncrementarCuotasPendientes();
            //    movimientos.Add(movProy);
            //}

            //movimientos.AddRange(Movimientos.Where(a => a.EsMensual).ToList());

            //var resumenM = _resumenAgrupadoM.Clone() as ResumenModel;
            //resumenM.FechaDeCierre = resumenM.FechaDeCierre.AddMonths(1);

            //var movimientosVM = new MovimientosViewModel(resumenM, _movimientosDA, movimientos);
            //OnPublishViewModelEvent(movimientosVM);
        }

        private void CargarMovimientos()
        {
            var movimientos = _movimientosDA.GetMovimientos(_resumenAgrupadoM);

            _movimientos = new ObservableCollection<BaseMovimiento>(movimientos);

            _agrupaciones = GetAgrupaciones(_movimientos);

            foreach (var item in _movimientos)
            {
                item.IsSelectedChangedEvent += Item_IsSelectedChangedEvent;
            }

            OnPropertyChanged("TotalResumen");
        }

        private void Item_IsSelectedChangedEvent(object sender, IsSelectedChangedEventHandler e)
        {
            if (Movimientos.Count(a => a.IsSelected) > 1)
            {
                AgrupacionesSeleccion = GetAgrupaciones(Movimientos.Where(a => a.IsSelected));                
            }
            else
            {
                AgrupacionesSeleccion = null;
            }
            OnPropertyChanged("TotalSeleccion");
        }

        private IEnumerable<AgrupacionItem> GetAgrupaciones(IEnumerable<BaseMovimiento> movimientos)
        {
            var tags = new List<AgrupacionItem>();
            foreach (var mov in movimientos.Where(a => a.Tags.Any()))
            {
                foreach (var tag in mov.Tags)
                {
                    if (tags.Exists(a => a.Descripcion.Equals(tag)))
                    {
                        tags.First(a => a.Descripcion.Equals(tag)).Monto += mov.Monto;
                    }
                    else
                    {
                        if (!tag.IsNullOrEmpty())
                        {
                            var newTag = new AgrupacionItem
                            {
                                Descripcion = tag,
                                Monto = mov.Monto
                            };
                            tags.Add(newTag);
                        }
                        else
                        {
                            Debug.WriteLine("Tag vacío" + mov.IdMovimiento);
                        }
                    }
                }
            }
            return tags.OrderByDescending(a => a.Monto);
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
