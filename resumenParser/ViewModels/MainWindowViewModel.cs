using ampersand.Core;
using ampersand.Core.Common;
using ResumenParser.Models;
using ResumenParser.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ResumenParser.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
        {
            Tags = new List<Tag>()
            {
                Tag.Get("nafta"),
                Tag.Get("chinos"),
                Tag.Get("super"),
                Tag.Get("ropa"),
                Tag.Get("donado")
            };
        }

        private string _filePath = Settings.Default.LastFilePath;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged("FilePath"); }
        }

        private string _resumenText = string.Empty;
        public string ResumenText
        {
            get { return _resumenText; }
            set { _resumenText = value; OnPropertyChanged("FileLines"); }
        }

        private string _periodo = null;
        public string Periodo
        {
            get
            {
                if (_periodo == null)
                    _periodo = DateTime.Today.Year.ToString() + (DateTime.Today.Month - 1).ToString("00");
                return _periodo;
            }
            set
            {
                _periodo = value;
                OnPropertyChanged("Periodo");
            }
        }

        private string _fechaDeCierre = null;
        public string FechaDeCierre
        {
            get
            {
                if (_fechaDeCierre == null)
                {
                    var fechaDeCierre = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-10);
                    _fechaDeCierre = fechaDeCierre.Year.ToString() + fechaDeCierre.Month.ToString("00") + fechaDeCierre.Day.ToString("00");
                }
                return _fechaDeCierre;
            }
            set
            {
                _fechaDeCierre = value;
                OnPropertyChanged("FechaDeCierre");
            }
        }

        private string _parserResult = string.Empty;
        public string ParserResult
        {
            get { return _parserResult; }
            set { _parserResult = value; OnPropertyChanged("ParserResult"); }
        }

        private IEnumerable<Movimiento> _movimientos = Enumerable.Empty<Movimiento>();
        public IEnumerable<Movimiento> Movimientos
        {
            get
            {
                if (_movimientos == null)
                    _movimientos = Enumerable.Empty<Movimiento>();
                return _movimientos;
            }
            set
            {
                _movimientos = value;
                OnPropertyChanged("Movimientos");
            }
        }

        public IEnumerable<Tag> Tags { get; private set; }

        private ICommand _aplicarTagsCommand;
        public ICommand AplicarTagsCommand
        {
            get
            {
                if (_aplicarTagsCommand == null)
                    _aplicarTagsCommand = new RelayCommand(a => AplicarTagsCommandExecute(),
                                                           a => AplicarTagsCommandCanExecute());
                return _aplicarTagsCommand;
            }
        }

        private void AplicarTagsCommandExecute()
        {
            var tagsSelected = Tags.Where(a => a.IsSelected).Select(a => a.Name);
            var movimientosSeleccionados = Movimientos.Where(a => a.IsSelected);

            foreach (var movimiento in movimientosSeleccionados)
                movimiento.Tags = string.Join(";", tagsSelected);

            Tags.ToList().ForEach(c => c.IsSelected = false);
        }

        private bool AplicarTagsCommandCanExecute()
        {
            return Movimientos.Any(a => a.IsSelected);
        }

        private ICommand _convertirCommand;
        public ICommand ConvertirCommand
        {
            get
            {
                if (_convertirCommand == null)
                    _convertirCommand = new RelayCommand(a => ConvertirCommandExecute());
                return _convertirCommand;
            }
        }

        private void ConvertirCommandExecute()
        {
            var stringLines = new List<string>();
            using (StringReader reader = new StringReader(ResumenText))
            {
                var line = string.Empty;
                while ((line = reader.ReadLine()).IsNullOrEmpty() == false)
                {
                    stringLines.Add(line);
                }
            }

            var parser = new Parser();

            try
            {
                var movimientos = parser.GetMovimientos(stringLines);
                Movimientos = movimientos;

                var xdoc = parser.GetXDocument(Movimientos, Periodo, FechaDeCierre);
                ParserResult = xdoc.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(a => SaveComandExecute(), a => SaveCommandCanExecute());
                return _saveCommand;
            }
        }

        private void SaveComandExecute()
        {
            var parser = new Parser();
            var xdoc = parser.GetXDocument(Movimientos, Periodo, FechaDeCierre);

            var filePath = Win32Helper.ShowFileDialog(Periodo);
            if (!filePath.IsNullOrEmpty())
            {
                try
                {
                    Win32Helper.SaveFile(filePath, xdoc);
                }
                catch (Exception ex)
                {
                    Win32Helper.ShowError(ex);
                }
            }
        }

        private bool SaveCommandCanExecute()
        {
            var canExecute = !Movimientos.Any(a => a.Error.Length > 0);

            return canExecute;
        }

    }
}
