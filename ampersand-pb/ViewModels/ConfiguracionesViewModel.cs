using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.DataAccess;
using ampersand_pb.Models;
using ampersand_pb.Properties;
using MahApps.Metro.Controls.Dialogs;

namespace ampersand_pb.ViewModels
{
    public class ConfiguracionesViewModel : BaseViewModel, IDataErrorInfo
    {
        public ConfiguracionesViewModel(ConfiguracionModel configuracionM, IConfiguracionDataAccess configuracionDA)
        {
            Header = "Configuraciones";
            _configuracionOriginal = configuracionM;
            _configuracionM = configuracionM.Clone();
            _configuracionDA = configuracionDA;

            TiposDeMediosDePago = new List<TiposDeMovimiento>() { TiposDeMovimiento.Credito, TiposDeMovimiento.Efectivo, TiposDeMovimiento.Debito };

            Temas = new List<ThemesColors>() { ThemesColors.Light, ThemesColors.Dark };
        }

        private IConfiguracionDataAccess _configuracionDA;
        private ConfiguracionModel _configuracionOriginal;
        private ConfiguracionModel _configuracionM;

        public string Header { get; private set; }

        public IEnumerable<TiposDeMovimiento> TiposDeMediosDePago { get; private set; }

        public string CarpetaDeResumenes
        {
            get { return _configuracionM.CarpetaDeResumenes; }
            set { _configuracionM.CarpetaDeResumenes = value; OnPropertyChanged("CarpetaDeResumenes"); }
        }

        public bool MostrarMediosDePago
        {
            get
            {
                return _configuracionM.CarpetaDeResumenesValida;
            }
        }

        public IEnumerable<PagoModel> MediosDePagos
        {
            get
            {
                return _configuracionM.MediosDePago;
            }
        }

        public bool VerCheckDeVerificacion
        {
            get
            {
                return Settings.Default.VerCheckDeVerificacion;
            }
            set
            {
                Settings.Default.VerCheckDeVerificacion = value;
                OnPropertyChanged("VerCheckDeVerificacion");
            }
        }

        public IEnumerable<ThemesColors> Temas { get; private set; }

        private ThemesColors _temaSeleccionado;
        public ThemesColors TemaSeleccionado
        {
            get
            {
                return _temaSeleccionado;
            }
            set
            {
                _temaSeleccionado = value;
                OnPropertyChanged("TemaSeleccionado");
            }
        }

        public bool HuboCambios
        {
            get
            {
                return !_configuracionM.Equals(_configuracionOriginal);
            }
        }

        public IDialogCoordinator DialogCoordinator { get; set; }

        private ICommand _buscarCarpetaCommand;
        public ICommand BuscarCarpetaCommand
        {
            get
            {
                if (_buscarCarpetaCommand == null)
                {
                    _buscarCarpetaCommand = new RelayCommand(p => BuscarCarpetaCommandExecute());
                }

                return _buscarCarpetaCommand;
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

        public string Error
        {
            get
            {
                return this["CarpetaDeResumenes"].IsNullOrEmpty() ?
                    "" :
                    "CarpetaDeResumenes";
            }
        }

        public string this[string propertyName]
        {
            get
            {
                return Validar(propertyName);
            }
        }

        private string Validar(string propertyName)
        {
            var error = string.Empty;

            if (propertyName.Equals("CarpetaDeResumenes"))
            {
                error = Directory.Exists(CarpetaDeResumenes) ?
                    string.Empty :
                    "No es una carpeta válida";
            }

            return error;
        }

        private void BuscarCarpetaCommandExecute()
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                var result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    CarpetaDeResumenes = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private bool SaveCommandCanExecute()
        {
            return HuboCambios && _configuracionM.CarpetaDeResumenesValida;
        }

        private void SaveCommandExecute()
        {
            _configuracionDA.GuardarConfiguracion(_configuracionM);

            _configuracionOriginal.CopyValues(_configuracionM);

            Properties.Settings.Default.Tema = TemaSeleccionado.ToString();
            Properties.Settings.Default.Save();

            ampersand_pb.Common.ResourceManager.AplicarTema();

            OnSaveEvent();
            CloseCommand.Execute(null);
        }

        #region Events

        public event EventHandler<ConfiguracionesViewModelSaveEventArgs> SaveEvent;
        private void OnSaveEvent()
        {
            this.SaveEvent?.Invoke(this, new ConfiguracionesViewModelSaveEventArgs(_configuracionOriginal));
        }

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;

        #endregion
    }

    public class ConfiguracionesViewModelSaveEventArgs : EventArgs
    {
        public ConfiguracionesViewModelSaveEventArgs(ConfiguracionModel configuracionM)
        {
            ConfiguracionM = configuracionM;
        }

        private ConfiguracionModel ConfiguracionM { get; }
    }
}
