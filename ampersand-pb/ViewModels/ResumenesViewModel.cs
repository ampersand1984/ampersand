using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class ResumenesViewModel : BaseViewModel, IMainWindowItem
    {
        public ResumenesViewModel(IMovimientosDataAccess movimientosDA)
        {
            _movimientosDA = movimientosDA;

            Resumenes = _movimientosDA.GetResumenes();
        }

        private IMovimientosDataAccess _movimientosDA;

        public string DisplayName
        {
            get
            {
                return "Resúmenes";
            }
        }

        public IEnumerable<ResumenModel> Resumenes { get; set; }

        private ICommand _seleccionarResumenCommand;
        public ICommand SeleccionarResumenCommand
        {
            get
            {
                if (_seleccionarResumenCommand == null)
                    _seleccionarResumenCommand = new RelayCommand(param => SeleccionarResumenCommandExecute(param as ResumenModel));
                return _seleccionarResumenCommand;
            }
        }

        private void SeleccionarResumenCommandExecute(ResumenModel resumenM)
        {
            if (resumenM != null)
            {
                var movimientosVM = new MovimientosViewModel(resumenM, _movimientosDA);
                OnPublishViewModelEvent(movimientosVM);
            }
        }

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
        private void OnPublishViewModelEvent(BaseViewModel viewModel)
        {
            var handler = this.PublishViewModelEvent;
            if (handler != null)
                handler(this, new PublishViewModelEventArgs(viewModel));
        }
    }
}
