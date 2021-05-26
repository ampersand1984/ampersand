using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using MahApps.Metro.Controls.Dialogs;
using Serilog;

namespace ampersand_pb.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
            : this(new ConfiguracionDataAccess(), dialogCoordinator) { }

        public MainWindowViewModel(IConfiguracionDataAccess configuracionDA, IDialogCoordinator dialogCoordinator)
        {
            Log.Information("MainWindowViewModel start");

            _dialogCoordinator = dialogCoordinator;

            _configuracionDA = configuracionDA;

            _configuracionM = _configuracionDA.GetConfiguracion();

            ResourceManager.AplicarTema();
        }

        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly ConfiguracionModel _configuracionM;

        private readonly IConfiguracionDataAccess _configuracionDA;

        private IMovimientosDataAccess _movimientosDA;
        public IMovimientosDataAccess MovimientosDA
        {
            get
            {
                if (_movimientosDA == null)
                    _movimientosDA = new MovimientosDataAccess(_configuracionM);
                return _movimientosDA;
            }
            set
            {
                _movimientosDA = value;
            }
        }

        #region Commands

        private ICommand _mostrarResumenesCommand;
        public ICommand MostrarResumenesCommand
        {
            get
            {
                if (_mostrarResumenesCommand == null)
                    _mostrarResumenesCommand = new RelayCommand(param => MostrarResumenesCommandExecute(), param => _configuracionM.CarpetaDeResumenesValida);
                return _mostrarResumenesCommand;
            }
        }

        private ICommand _mostrarResumenesGraficosCommand;
        public ICommand MostrarResumenesGraficosCommand
        {
            get
            {
                if (_mostrarResumenesGraficosCommand == null)
                    _mostrarResumenesGraficosCommand = new RelayCommand(param => MostrarResumenesGraficosCommandExecute(), param => _configuracionM.CarpetaDeResumenesValida);
                return _mostrarResumenesGraficosCommand;
            }
        }

        private ICommand _mostrarActualCommand;
        public ICommand MostrarActualCommand
        {
            get
            {
                if (_mostrarActualCommand == null)
                    _mostrarActualCommand = new RelayCommand(param => MostrarActualCommandExecuteAsync(), param => _configuracionM.CarpetaDeResumenesValida);
                return _mostrarActualCommand;
            }
        }

        private ICommand _mostrarConfiguracionesCommand;
        public ICommand MostrarConfiguracionesCommand
        {
            get
            {
                if (_mostrarConfiguracionesCommand == null)
                    _mostrarConfiguracionesCommand = new RelayCommand(param => MostrarConfiguracionesCommandExecute());
                return _mostrarConfiguracionesCommand;
            }
        }

        private ICommand _closeCurrentMainWindowItemCommand;
        public ICommand CloseCurrentMainWindowItemCommand
        {
            get
            {
                if (_closeCurrentMainWindowItemCommand == null)
                    _closeCurrentMainWindowItemCommand = new RelayCommand(param => CloseCurrentMainWindowItemCommandExecute());
                return _closeCurrentMainWindowItemCommand;
            }
        }

        private ICommand _buscarCommand;
        public ICommand BuscarCommand
        {
            get
            {
                if (_buscarCommand == null)
                    _buscarCommand = new RelayCommand(param => BuscarCommandExecute());
                return _buscarCommand;
            }
        }

        #endregion

        private BaseViewModel _editViewModel;
        public BaseViewModel EditViewModel
        {
            get
            {
                return _editViewModel;
            }
            set
            {
                _editViewModel = value;
                FlyoutIsOpen = _editViewModel != null;
                OnPropertyChanged("EditViewModel");
            }
        }

        private bool _flyoutIsOpen;
        public bool FlyoutIsOpen
        {
            get
            {
                return _flyoutIsOpen;
            }
            set
            {
                _flyoutIsOpen = value;
                if (!_flyoutIsOpen && EditViewModel != null)
                    EditViewModel.CloseCommand.Execute(null);
                OnPropertyChanged("FlyoutIsOpen");
            }
        }

        public bool VerVistaDeBienvenida
        {
            get
            {
                return _configuracionM.CarpetaDeResumenesValida && MainWindowItems.Count == 0;
            }
        }

        private BienvenidaViewModel _bienvenidaVM;
        public BienvenidaViewModel BienvenidaVM
        {
            get
            {
                if (_bienvenidaVM == null && VerVistaDeBienvenida)
                    _bienvenidaVM = new BienvenidaViewModel(MovimientosDA, _configuracionM);

                return _bienvenidaVM;
            }
        }

        private readonly ObservableCollection<IMainWindowItem> _mainWindowItems = new ObservableCollection<IMainWindowItem>();
        public ObservableCollection<IMainWindowItem> MainWindowItems
        {
            get { return _mainWindowItems; }
        }

        private IMainWindowItem _currentMainWindowItem;
        public IMainWindowItem CurrentMainWindowItem
        {
            get { return _currentMainWindowItem; }
            set { _currentMainWindowItem = value; OnPropertyChanged("CurrentMainWindowItem"); }
        }

        private void MostrarResumenesCommandExecute()
        {
            var resumenesVM = MainWindowItems.OfType<ResumenesViewModel>().FirstOrDefault();
            if (resumenesVM != null)
            {
                CurrentMainWindowItem = resumenesVM;
            }
            else
            {
                resumenesVM = new ResumenesViewModel(MovimientosDA, _configuracionM);

                AgregarMainWindowItem(resumenesVM);
            }
        }

        private void MostrarResumenesGraficosCommandExecute()
        {
            var resumenesVM = MainWindowItems.OfType<ResumenesGraficosViewModel>().FirstOrDefault();
            if (resumenesVM != null)
            {
                CurrentMainWindowItem = resumenesVM;
            }
            else
            {
                resumenesVM = new ResumenesGraficosViewModel(MovimientosDA, _configuracionM);

                AgregarMainWindowItem(resumenesVM);
            }
        }

        private void ViewModelCloseEvent(object sender, EventArgs e)
        {
            var mainWindowItem = sender as IMainWindowItem;
            mainWindowItem.CloseEvent -= ViewModelCloseEvent;
            mainWindowItem.PublishViewModelEvent -= PublishViewModelEvent;
            MainWindowItems.Remove(mainWindowItem);
            OnPropertyChanged("CurrentMainWindowItem");
            OnPropertyChanged("VerVistaDeBienvenida");
        }

        private void PublishViewModelEvent(object sender, PublishViewModelEventArgs e)
        {
            var mainWindowItem = e.ViewModel as IMainWindowItem;
            if (mainWindowItem != null)
                AgregarMainWindowItem(mainWindowItem);
            else
                AgregarEditVM(e.ViewModel);
        }

        private void AgregarMainWindowItem(IMainWindowItem mainWindowItem)
        {
            if (MainWindowItems.Any(a => a.DisplayName.Equals(mainWindowItem.DisplayName)))
            {
                CurrentMainWindowItem = MainWindowItems.First(a => a.DisplayName.Equals(mainWindowItem.DisplayName));
                mainWindowItem = null;
            }
            else
            {
                mainWindowItem.PublishViewModelEvent += PublishViewModelEvent;
                mainWindowItem.CloseEvent += ViewModelCloseEvent;

                mainWindowItem.DialogCoordinator = _dialogCoordinator;

                MainWindowItems.Add(mainWindowItem);
                CurrentMainWindowItem = _mainWindowItems.LastOrDefault();
                OnPropertyChanged("VerVistaDeBienvenida");
            }
        }

        private async Task ShowMessage(MessageParam messageParam)
        {
            await _dialogCoordinator.ShowMessageAsync(this, messageParam.Title, messageParam.Message, messageParam.Style, messageParam.Settings);
        }

        private void MostrarActualCommandExecuteAsync()
        {
            var resumenAgrupado = MovimientosDA.GetUltimoResumen();
            if (resumenAgrupado != null)
            {
                var movimientosVM = new MovimientosViewModel(resumenAgrupado, MovimientosDA, _configuracionM) { DialogCoordinator = _dialogCoordinator };
                AgregarMainWindowItem(movimientosVM);
            }
        }

        private void BuscarCommandExecute()
        {
            var movimientosVM = new BuscarMovimentosViewModel(MovimientosDA, _configuracionM) { DialogCoordinator = _dialogCoordinator };
            AgregarMainWindowItem(movimientosVM);
        }

        private void MostrarConfiguracionesCommandExecute()
        {
            var configuracionesVM = new ConfiguracionesViewModel(_configuracionM, _configuracionDA);
            configuracionesVM.SaveEvent += ConfiguracionesVM_SaveEvent;
            configuracionesVM.CloseEvent += ConfiguracionesVM_CloseEvent;

            AgregarEditVM(configuracionesVM);
        }

        private void ConfiguracionesVM_SaveEvent(object sender, ConfiguracionesViewModelSaveEventArgs e)
        {
            //_filesPath = e.FilesPath;
            //_filesPathValido = Directory.Exists(_filesPath);
        }

        private void ConfiguracionesVM_CloseEvent(object sender, EventArgs e)
        {
            var configuracionesVM = sender as ConfiguracionesViewModel;
            configuracionesVM.SaveEvent -= ConfiguracionesVM_SaveEvent;
            configuracionesVM.CloseEvent -= ConfiguracionesVM_CloseEvent;
        }

        private void CloseCurrentMainWindowItemCommandExecute()
        {
            if (CurrentMainWindowItem != null)
            {
                if (EditViewModel != null)
                    EditViewModel.CloseCommand.Execute(null);
                else
                    CurrentMainWindowItem.CloseCommand.Execute(null);
            }
        }

        private void EditViewModel_CloseEvent(object sender, EventArgs e)
        {
            EditViewModel.CloseEvent -= new EventHandler(EditViewModel_CloseEvent);
            EditViewModel.Dispose();
            EditViewModel = null;
        }

        private void AgregarEditVM(BaseViewModel editVM)
        {
            EditViewModel = editVM;
            EditViewModel.CloseEvent += new EventHandler(EditViewModel_CloseEvent);
        }
    }

    public class ActionItem
    {
        public ICommand Command { get; set; }

        public string Description { get; set; }
    }
}
