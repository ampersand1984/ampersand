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
        public MovimientosViewModel(IMovimientosDataAccess movimientosDA, ResumenModel resumenM)
        {
            _movimientosDA = movimientosDA;
            _resumenM = resumenM;
        }

        public MovimientosViewModel(IEnumerable<BaseMovimiento> movimientos, ResumenModel resumenM)
        {
            _movimientos = new ObservableCollection<BaseMovimiento>(movimientos);
            _resumenM = resumenM;

            CargarAgrupaciones();
        }

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

            var movimientosVM = new MovimientosViewModel(movimientos, resumenM);
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
