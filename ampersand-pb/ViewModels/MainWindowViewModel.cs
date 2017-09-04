﻿using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
            :this(new ConfiguracionDataAccess()) { }

        public MainWindowViewModel(IConfiguracionDataAccess configuracionDA)
        {
            _configuracionDA = configuracionDA;

            _configuracionM = _configuracionDA.GetConfiguracion();

            ActionList = new List<ActionItem>()
            {
                new ActionItem
                {
                    Description = "Último",
                    Command = MostrarActualCommand
                },
                new ActionItem
                {
                    Description = "Gráficos por mes",
                    Command = this.MostrarResumenesGraficosCommand
                },
                new ActionItem
                {
                    Description = "Resúmenes",
                    Command = this.MostrarResumenesCommand
                },
                new ActionItem
                {
                    Description = "Configuraciones",
                    Command = MostrarConfiguracionesCommand
                }
            };
        }
        
        private ConfiguracionModel _configuracionM;

        private IConfiguracionDataAccess _configuracionDA;

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

        public IEnumerable<ActionItem> ActionList { get; private set; }

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
                    _mostrarActualCommand = new RelayCommand(param => MostrarActualCommandExecute(), param => _configuracionM.CarpetaDeResumenesValida);
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

        private void MostrarResumenesGraficosCommandExecute()
        {
            var resumenesVM = MainWindowItems.OfType<ResumenesGraficosViewModel>().FirstOrDefault();
            if (resumenesVM != null)
            {
                CurrentMainWindowItem = resumenesVM;
            }
            else
            {
                resumenesVM = new ResumenesGraficosViewModel(MovimientosDA);

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
            if (MainWindowItems.Any(a => a.DisplayName.Equals(mainWindowItem.DisplayName)))
            {
                CurrentMainWindowItem = MainWindowItems.First(a => a.DisplayName.Equals(mainWindowItem.DisplayName));
                mainWindowItem = null;
            }
            else
            {
                mainWindowItem.PublishViewModelEvent += PublishViewModelEvent;
                mainWindowItem.CloseEvent += ViewModelCloseEvent;
                MainWindowItems.Add(mainWindowItem);
                CurrentMainWindowItem = _mainWindowItems.LastOrDefault();
            }
        }

        private void MostrarActualCommandExecute()
        {
            var resumenes = MovimientosDA.GetResumenes();
            if (resumenes.Any())
            {
                var resumenesAgrupados = resumenes.Agrupar();
                var movimientosVM = new MovimientosViewModel(resumenesAgrupados.First(), MovimientosDA);
                AgregarMainWindowItem(movimientosVM);
            }
        }

        private void MostrarConfiguracionesCommandExecute()
        {
            var configuracionesVM = new ConfiguracionesViewModel(_configuracionM, _configuracionDA);
            configuracionesVM.SaveEvent += ConfiguracionesVM_SaveEvent;
            configuracionesVM.CloseEvent += ConfiguracionesVM_CloseEvent;

            AgregarMainWindowItem(configuracionesVM);
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
                CurrentMainWindowItem.CloseCommand.Execute(null);
        }

        private void EditViewModel_CloseEvent(object sender, EventArgs e)
        {
            EditViewModel.CloseEvent -= new EventHandler(EditViewModel_CloseEvent);
            EditViewModel.Dispose();
            EditViewModel = null;
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
