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
        public MovimientosViewModel(ResumenModel resumenM, IMovimientosDataAccess movimientosDA)
        {
            _resumenM = resumenM;
            _movimientosDA = movimientosDA;
        }

        public MovimientosViewModel(ResumenModel resumenM, IMovimientosDataAccess movimientosDA, IEnumerable<BaseMovimiento> movimientos)
            :this(resumenM, movimientosDA)
        {
            _esProyeccion = true;

            _movimientos = new ObservableCollection<BaseMovimiento>(movimientos);

            CargarAgrupaciones();
        }

        private bool _esProyeccion;

        private IMovimientosDataAccess _movimientosDA;
        private ResumenModel _resumenM;

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
                return _resumenM.TextoPeriodo;
            }
        }

        public decimal TotalResumen
        {
            get { return  Movimientos.Sum(a => a.Monto); }
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

        public bool EsElUtimoMes
        {
            get
            {
                return _resumenM.EsElUtimoMes;
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
            get { return _editarMovimientoCommand ?? (_editarMovimientoCommand = new RelayCommand(param => this.EditarMovimientoCommandExecute())); }
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

        private bool ImportarInfoDeResumenAnteriorCommandCanExecute()
        {
            return EsElUtimoMes && _esProyeccion == false;
        }

        private void ImportarInfoDeResumenAnteriorCommandExecute()
        {
            var movimientosAnteriores = _movimientosDA.GetMovimientosDeResumenAnterior(_resumenM.FechaDeCierre);
            foreach (var movAnterior in movimientosAnteriores)
            {
                var movActual = Movimientos.FirstOrDefault(a => a.IdMovimiento.Equals(movAnterior.IdMovimiento) &&
                                                                a.Descripcion.Equals(movAnterior.Descripcion));
                if (movActual != null)
                {
                    movActual.Tags = movAnterior.Tags;
                    movActual.DescripcionAdicional = movAnterior.DescripcionAdicional;
                    movActual.EsMensual = movAnterior.EsMensual;
                    movActual.EsAjeno = movAnterior.EsAjeno;
                }                
            }
        }
        

        protected override void OnRequestCloseEvent()
        {
            if (EditVM != null)
                EditVM.CloseCommand.Execute(null);
            else
                base.OnRequestCloseEvent();
        }

        private void EditarMovimientoCommandExecute()
        {
            if (SelectedIndex != -1)
            {
                var tags = Movimientos.SelectMany(a => a.Tags).Distinct().GetTags();

                var baseMovimiento = Movimientos.ElementAt(SelectedIndex);
                var movimientoABMVM = new MovimientoABMViewModel(baseMovimiento, tags);
                movimientoABMVM.CloseEvent += (sender, e) =>
                    {
                        EditVM = null;
                    };
                EditVM = movimientoABMVM;
            }
        }

        private bool ProyectarCommandCanExecute()
        {
            return EsElUtimoMes;
        }

        private void ProyectarCommandExecute()
        {
            var movimientos = new List<BaseMovimiento>();

            foreach (var mov in Movimientos.Where(a => a.CoutasPendientes > 0)
                                           .OrderByDescending(a => a.CoutasPendientes))
            {
                var movProy = mov.Clone() as BaseMovimiento;
                movProy.IncrementarCuotasPendientes();
                movimientos.Add(movProy);
            }

            movimientos.AddRange(Movimientos.Where(a => a.EsMensual).ToList());

            var resumenM = _resumenM.Clone() as ResumenModel;
            resumenM.FechaDeCierre = resumenM.FechaDeCierre.AddMonths(1);

            var movimientosVM = new MovimientosViewModel(resumenM, _movimientosDA, movimientos);
            OnPublishViewModelEvent(movimientosVM);
        }

        private void CargarMovimientos()
        {
            var movimientos = _movimientosDA.GetMovimientos(_resumenM.FilePath, _resumenM.Periodo);

            _movimientos = new ObservableCollection<BaseMovimiento>(movimientos);

            CargarAgrupaciones();

            OnPropertyChanged("TotalResumen");
        }

        private void CargarAgrupaciones()
        {
            var tags = new List<AgrupacionItem>();
            foreach (var mov in Movimientos.Where(a => a.Tags.Any()))
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
            _agrupaciones = tags.OrderBy(a => a.Descripcion);
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
