using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class MovimientoABMViewModel: BaseViewModel
    {
        #region Constructor

        public MovimientoABMViewModel(ConfiguracionModel configuracionM)
            :this(new BaseMovimiento(), configuracionM)
        {
            _modelOriginal.IdResumen = configuracionM.MediosDePago.First().Id;
            _modelOriginal.DescripcionResumen = configuracionM.MediosDePago.First().Descripcion;
        }

        public MovimientoABMViewModel(BaseMovimiento baseMovimiento, ConfiguracionModel configuracionM, bool esCopia = false)
        {
            EdicionCompleta = true;
            _modelOriginal = baseMovimiento;
            _model = baseMovimiento.Clone() as BaseMovimiento;

            _configuracionM = configuracionM;

            _esCopia = esCopia;

            _tagsPosibles = _configuracionM.Tags.Clone();
        }

        #endregion

        #region Fields

        private BaseMovimiento _modelOriginal;
        private BaseMovimiento _model;
        private IEnumerable<TagModel> _tagsPosibles;
        ConfiguracionModel _configuracionM;
        private bool _guardado;
        private bool _esCopia;

        #endregion

        #region Properties

        public string Header
        {
            get
            {
                return Descripcion.IsNullOrEmpty() || _esCopia?
                    "Nuevo" :
                    Descripcion;
            }
        }

        public bool EdicionCompleta { get; private set; }

        public int IdMovimiento
        {
            get { return _model.IdMovimiento; }
            set { _model.IdMovimiento = value; OnPropertyChanged("IdMovimiento"); }
        }

        public PagoModel MedioDePago
        {
            get
            {
                if (_model.IdResumen.IsNullOrEmpty())
                {
                    _model.IdResumen = MediosDePago.First().Id;
                    _model.DescripcionResumen = MediosDePago.First().Descripcion;
                }
                return MediosDePago.FirstOrDefault(a => a.Id == _model.IdResumen);
            }
            set
            {
                if (value != null)
                {
                    _model.IdResumen = value.Id;
                    _model.DescripcionResumen = value.Descripcion;
                }
                OnPropertyChanged("MedioDePago");
            }
        }

        public IEnumerable<PagoModel> MediosDePago
        {
            get
            {
                return _configuracionM.MediosDePago;
            }
        }

        public DateTime Fecha
        {
            get { return _model.Fecha; }
            set { _model.Fecha = value; OnPropertyChanged("Fecha"); }
        }

        public string Descripcion
        {
            get { return _model.Descripcion; }
            set { _model.Descripcion = value; OnPropertyChanged("Descripcion"); }
        }

        public string DescripcionAdicional
        {
            get { return _model.DescripcionAdicional; }
            set { _model.DescripcionAdicional = value; OnPropertyChanged("DescripcionAdicional"); }
        }

        public decimal Monto
        {
            get { return _model.Monto; }
            set { _model.SetMonto(value); RefrescarMontos(); }
        }

        public decimal MontoME
        {
            get { return _model.MontoME; }
            set { _model.SetMontoME(value, _model.Cotizacion); RefrescarMontos(); }
        }

        public decimal Cotizacion
        {
            get { return _model.Cotizacion; }
        }

        public bool EsMonedaExtranjera
        {
            get
            {
                return _model.EsMonedaExtranjera;
            }
        }

        public void RefrescarMontos()
        {
            OnPropertyChanged("Monto");
            OnPropertyChanged("MontoME");
            OnPropertyChanged("EsMonedaExtranjera");
            OnPropertyChanged("Cotizacion");
            OnPropertyChanged("TotalCuotas");
        }

        public string Cuota
        {
            get { return _model.Cuota; }
            set { _model.Cuota = value; OnPropertyChanged("Cuota"); OnPropertyChanged("TotalCuotas"); }
        }

        public IEnumerable<string> Cuotas
        {
            get
            {
                return new List<string>()
                    {
                        "",
                        "01/02",
                        "01/03",
                        "01/06",
                        "01/12",
                        "01/18",
                        "01/24",
                        "01/50",
                        _model.Cuota
                    }.Distinct();
            }
        }

        public string TotalCuotas
        {
            get
            {
                var totalCuotas = string.Empty;

                if (Cuota.Length > 0)
                {
                    var slashIndex = Cuota.IndexOf("/");
                    var cantCuotas = int.Parse(Cuota.Substring(slashIndex + 1));
                    totalCuotas = $"Total: {(Monto * cantCuotas).ToString("C2")}";
                }

                return totalCuotas;
            }
        }

        public bool EsMensual
        {
            get { return _model.EsMensual; }
            set { _model.EsMensual = value; OnPropertyChanged("EsMensual"); }
        }

        public bool EsAjeno
        {
            get { return _model.EsAjeno; }
            set { _model.EsAjeno = value; OnPropertyChanged("EsAjeno"); }
        }

        private IEnumerable<TagModel> _tags;
        public IEnumerable<TagModel> Tags
        {
            get
            {
                return _tags ?? (_tags = GetTags());
            }
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new RelayCommand(para => SaveCommandExecute(), param => SaveCommandCanExecute());
                return _saveCommand;
            }
        }

        #endregion

        #region Methods

        private IEnumerable<TagModel> GetTags()
        {
            var tags = _model.Tags.GetTags(true).ToList();

            foreach (var tagPosible in _tagsPosibles)
            {
                if (!tags.Any(a => a.Tag.Equals(tagPosible.Tag)))
                    tags.Add(tagPosible);
            }

            return tags.OrderByDescending(a => a.Seleccionada).ThenBy(a => a.Tag);
        }

        private bool SaveCommandCanExecute()
        {
            return true;
        }

        private void SaveCommandExecute()
        {
            var idResumenOriginal = _modelOriginal.IdResumen;
            _modelOriginal.CopyValues(_model);

            _modelOriginal.Tags = this.Tags.Where(a => a.Seleccionada).Select(a => a.Tag).ToList();
            _modelOriginal.RefrescarPropiedades();

            OnSaveEvent(idResumenOriginal);
            _guardado = true;
            CloseCommand.Execute(null);
        }

        protected override void OnRequestCloseEvent()
        {
            if (!_guardado)
            {
                var huboCambios = !_model.Equals(_modelOriginal);
                if (huboCambios)
                {
                    var result = MessageBox.Show("Salir sin guardar?", "Salir", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.No)
                        return;
                }
            }

            base.OnRequestCloseEvent();
        }

        #endregion

        #region Events

        public event EventHandler<MovimientoABMSaveEventArgs> SaveEvent;
        private void OnSaveEvent(string idResumenOriginal)
        {
            var handler = this.SaveEvent;
            if (handler != null)
                handler(this, new MovimientoABMSaveEventArgs(_modelOriginal, idResumenOriginal));
        }

        #endregion
    }

    public class MovimientoABMSaveEventArgs : EventArgs
    {
        public MovimientoABMSaveEventArgs(BaseMovimiento model, string idResumenOriginal)
        {
            this.Model = model;
            IdResumenOriginal = idResumenOriginal;
        }

        public BaseMovimiento Model { get; private set; }

        public string IdResumenOriginal { get; private set; }
    }
}
