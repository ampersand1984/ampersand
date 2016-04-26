using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ampersand_pb.ViewModels
{
    public class MovimientoABMViewModel: BaseViewModel
    {
        #region Constructor

        public MovimientoABMViewModel()
            :this(new BaseMovimiento())
        {
        }

        public MovimientoABMViewModel(BaseMovimiento baseMovimiento)
        {
            EdicionCompleta = true;
            _modelOriginal = baseMovimiento;
            _model = baseMovimiento.Clone() as BaseMovimiento;

            _tagsPosibles = new List<TagModel>()
            {
                new TagModel() { Tag = "super" },
                new TagModel() { Tag = "chinos" },
                new TagModel() { Tag = "nafta" },
                new TagModel() { Tag = "ropa" },
                new TagModel() { Tag = "donado" },
                new TagModel() { Tag = "auto" }
            };
        }

        #endregion

        #region Fields

        private BaseMovimiento _modelOriginal;
        private BaseMovimiento _model;
        private IEnumerable<TagModel> _tagsPosibles;

        #endregion

        #region Properties

        public bool EdicionCompleta { get; private set; }

        public int IdMovimiento
        {
            get { return _model.IdMovimiento; }
            set { _model.IdMovimiento = value; OnPropertyChanged("IdMovimiento"); }
        }

        public string TipoDescripcion
        {
            get
            {
                if (_model.TipoDescripcion.IsNullOrEmpty())
                    _model.TipoDescripcion = TipoDescripciones.First();
                return _model.TipoDescripcion;
            }
            set { _model.TipoDescripcion = value; OnPropertyChanged("TipoDescripcion"); }
        }

        public IEnumerable<string> TipoDescripciones
        {
            get
            {
                return new List<string>() { "Visa", "Master Card" };
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
            set { _model.Monto = value; OnPropertyChanged("Monto"); }
        }

        public string Cuota
        {
            get { return _model.Cuota; }
            set { _model.Cuota = value; OnPropertyChanged("Cuota"); }
        }

        public IEnumerable<string> Cuotas
        {
            get
            {
                if (EdicionCompleta)
                    return new List<string>()
                    {
                        "",
                        "1/2",
                        "1/3",
                        "1/6",
                        "1/12",
                        "1/18",
                        "1/24",
                        "1/50"
                    };
                else
                    return new List<string>() { _model.Cuota };

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
            get { return _tags ?? (_tags = GetTags()); }
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
            _modelOriginal.Descripcion = _model.Descripcion;
            _modelOriginal.DescripcionAdicional = _model.DescripcionAdicional;
            _modelOriginal.Fecha = _model.Fecha;
            _modelOriginal.IdMovimiento = _model.IdMovimiento;
            _modelOriginal.Tipo = _model.Tipo;
            _modelOriginal.TipoDescripcion = _model.TipoDescripcion;
            _modelOriginal.Cuota = _model.Cuota;
            _modelOriginal.Monto = _model.Monto;
            _modelOriginal.EsMensual = _model.EsMensual;
            _modelOriginal.EsAjeno = _model.EsAjeno;
            _modelOriginal.Tags = this.Tags.Where(a => a.Seleccionada).Select(a => a.Tag).ToList();

            OnSaveEvent();
            CloseCommand.Execute(null);
        }

        #endregion

        #region Events

        public event EventHandler<MovimientoABMSaveEventArgs> SaveEvent;
        private void OnSaveEvent()
        {
            var handler = this.SaveEvent;
            if (handler != null)
                handler(this, new MovimientoABMSaveEventArgs(_modelOriginal));
        }

        #endregion
    }

    public class MovimientoABMSaveEventArgs : EventArgs
    {
        public MovimientoABMSaveEventArgs(BaseMovimiento model)
        {
            this.Model = model;
        }

        public BaseMovimiento Model { get; private set; }
    }
}
