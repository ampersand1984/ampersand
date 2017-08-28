using ampersand.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ampersand_pb.DataAccess;
using System.Windows.Input;
using ampersand.Core.Common;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using ampersand_pb.Models;

namespace ampersand_pb.ViewModels
{
    public class ConfiguracionesViewModel : BaseViewModel, IMainWindowItem, IDataErrorInfo
    {
        public ConfiguracionesViewModel(IConfiguracionDataAccess configuracionDA)
        {
            DisplayName = "Configuraciones";
            _configuracionDA = configuracionDA;
            _carpetaDeResumenes = _configuracionDA.GetFilesPath();            

            if (Directory.Exists(CarpetaDeResumenes))
                CargarConfiguracion();
        }

        private void CargarConfiguracion()
        {

            _configuracionModel = _configuracionDA.GetConfiguracion(CarpetaDeResumenes);
        }

        private IConfiguracionDataAccess _configuracionDA;
        private ConfiguracionModel _configuracionModel;
        private bool _huboCambios;

        public string DisplayName { get; private set; }

        private string _carpetaDeResumenes;
        public string CarpetaDeResumenes
        {
            get { return _carpetaDeResumenes; }
            set { _carpetaDeResumenes = value; OnPropertyChanged("CarpetaDeResumenes"); }
        }

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
                    "":
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
                    string.Empty:
                    "No es una carpeta válida";
            }

            return error;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName.Equals("CarpetaDeResumenes"))
                _huboCambios = true;
        }

        private void BuscarCarpetaCommandExecute()
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                var result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    //MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

                    CarpetaDeResumenes = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private bool SaveCommandCanExecute()
        {
            return _huboCambios && !CarpetaDeResumenes.IsNullOrEmpty();
        }

        private void SaveCommandExecute()
        {
            _configuracionDA.SaveFilesPath(CarpetaDeResumenes);

            OnSaveEvent();
            CloseCommand.Execute(null);
        }

        #region Events

        public event EventHandler<ConfiguracionesViewModelSaveEventArgs> SaveEvent;
        private void OnSaveEvent()
        {
            var handler = this.SaveEvent;
            if (handler != null)
                handler(this, new ConfiguracionesViewModelSaveEventArgs(CarpetaDeResumenes));
        }

        public event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;

        #endregion
    }

    public class ConfiguracionesViewModelSaveEventArgs: EventArgs
    {
        public ConfiguracionesViewModelSaveEventArgs(string filesPath)
        {
            FilesPath = filesPath;
        }

        public string FilesPath { get; private set; }
    }
}
