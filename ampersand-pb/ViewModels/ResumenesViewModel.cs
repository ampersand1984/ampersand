using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class ResumenesViewModel : BaseViewModel, IMainWindowItem
    {
        public ResumenesViewModel(IMovimientosDataAccess movimientosDA, ConfiguracionModel configuracionM)
        {
            _movimientosDA = movimientosDA;

            _configuracionM = configuracionM;

            var resumenes = _movimientosDA.GetResumenes();

            ResumenesAgrupados = resumenes.Agrupar();
        }

        private IMovimientosDataAccess _movimientosDA;
        ConfiguracionModel _configuracionM;

        public string DisplayName
        {
            get
            {
                return "Resúmenes";
            }
        }

        public IEnumerable<ResumenAgrupadoModel> ResumenesAgrupados { get; private set; }

        public IDialogCoordinator DialogCoordinator { get; set; }

        private ICommand _seleccionarResumenCommand;
        public ICommand SeleccionarResumenCommand
        {
            get
            {
                if (_seleccionarResumenCommand == null)
                    _seleccionarResumenCommand = new RelayCommand(param => SeleccionarResumenCommandExecute(param as ResumenAgrupadoModel));
                return _seleccionarResumenCommand;
            }
        }

        private void SeleccionarResumenCommandExecute(ResumenAgrupadoModel resumenAgrupadoM)
        {
            if (resumenAgrupadoM != null)
            {
                var movimientosVM = new MovimientosViewModel(resumenAgrupadoM, _movimientosDA, _configuracionM);
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
