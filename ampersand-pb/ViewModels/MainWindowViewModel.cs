using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ampersand.Core;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System.Windows.Input;
using ampersand.Core.Common;
using System.Collections.ObjectModel;

namespace ampersand_pb.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
        {
            ActionList = new List<ActionItem>()
            {
                new ActionItem
                {
                    Description = "Resúmenes",
                    Command = this.MostrarResumenesCommand
                },
                new ActionItem
                {
                    Description = "Actual",
                    Command = this.MostrarActualCommand
                },
                new ActionItem
                {
                    Description = "Configuraciones",
                    Command = this.MostrarConfiguracionesCommand
                }
            };
        }

        private string _filesPath = @"C:\Google Drive\Resumen y comprobantes\Apb\";

        public IEnumerable<ActionItem> ActionList { get; private set; }

        private ICommand _mostrarResumenesCommand;
        public ICommand MostrarResumenesCommand
        {
            get
            {
                if (_mostrarResumenesCommand == null)
                    _mostrarResumenesCommand = new RelayCommand(param => MostrarResumenesCommandExecute());
                return _mostrarResumenesCommand;
            }
        }

        private ICommand _mostrarActualCommand;
        public ICommand MostrarActualCommand
        {
            get
            {
                if (_mostrarActualCommand == null)
                    _mostrarActualCommand = new RelayCommand(param => MostrarActualCommandExecute());
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
                if (_editViewModel != null)
                    _editViewModel.CloseEvent += new EventHandler(EditViewModel_CloseEvent);
                OnPropertyChanged("EditViewModel");
            }
        }

        private ObservableCollection<IMainWindowItem> _mainWindowItems = new ObservableCollection<IMainWindowItem>();
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
                resumenesVM = new ResumenesViewModel(MovimientosDA);

                AgregarMainWindowItem(resumenesVM);
            }
        }

        private void ViewModelCloseEvent(object sender, EventArgs e)
        {
            var mainWindowItem = sender as IMainWindowItem;
            mainWindowItem.PublishViewModelEvent -= PublishViewModelEvent;
            mainWindowItem.CloseEvent -= ViewModelCloseEvent;
            MainWindowItems.Remove(mainWindowItem);
            OnPropertyChanged("CurrentMainWindowItem");            
        }

        private void PublishViewModelEvent(object sender, PublishViewModelEventArgs e)
        {
            var mainWindowItem = e.ViewModel as IMainWindowItem;
            if (mainWindowItem != null)
                AgregarMainWindowItem(mainWindowItem);
        }

        private void AgregarMainWindowItem(IMainWindowItem mainWindowItem)
        {
            mainWindowItem.PublishViewModelEvent += PublishViewModelEvent;
            mainWindowItem.CloseEvent += ViewModelCloseEvent;
            MainWindowItems.Add(mainWindowItem);
            CurrentMainWindowItem = _mainWindowItems.LastOrDefault();
        }

        private void MostrarActualCommandExecute()
        {
            throw new NotImplementedException();
        }

        private object MostrarConfiguracionesCommandExecute()
        {
            throw new NotImplementedException();
        }

        private void CloseCurrentMainWindowItemCommandExecute()
        {
            if (CurrentMainWindowItem != null)
                CurrentMainWindowItem.CloseCommand.Execute(null);
        }

        private void EditViewModel_CloseEvent(object sender, EventArgs e)
        {
            EditViewModel.CloseEvent -= new EventHandler(EditViewModel_CloseEvent);
            EditViewModel.Dispose();
            EditViewModel = null;
        }

        private IMovimientosDataAccess _movimientosDA;
        public IMovimientosDataAccess MovimientosDA
        {
            get
            {
                if (_movimientosDA == null)
                    _movimientosDA = new MovimientosDataAccess(_filesPath);
                return _movimientosDA;
            }
            set
            {
                _movimientosDA = value;
            }
        }

        private void EditViewModelEvent(object sender, EditViewModelEventArgs e)
        {
            EditViewModel = e.ViewModel;
        }
    }

    public class ActionItem
    {
        public ICommand Command { get; set; }

        public string Description { get; set; }
    }
}
