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
        public MovimientoABMViewModel(BaseMovimiento baseMovimiento, IEnumerable<TagModel> tagsPosibles)
        {
            _modelOriginal = baseMovimiento;
            _model = baseMovimiento.Clone() as BaseMovimiento;

            _tagsPosibles = tagsPosibles;
        }

        private BaseMovimiento _modelOriginal;
        private BaseMovimiento _model;
        private IEnumerable<TagModel> _tagsPosibles;

        public int IdMovimiento
        {
            get { return _model.IdMovimiento; }
            set { _model.IdMovimiento = value; }
        }

        public DateTime Fecha
        {
            get { return _model.Fecha; }
            set { _model.Fecha = value; }
        }

        public string Descripcion
        {
            get { return _model.Descripcion; }
            set { _model.Descripcion = value; }
        }

        public string DescripcionAdicional
        {
            get { return _model.DescripcionAdicional; }
            set { _model.DescripcionAdicional = value; }
        }

        public decimal Monto
        {
            get { return _model.Monto; }
            set { _model.Monto = value; }
        }

        public string Cuota
        {
            get { return _model.Cuota; }
            set { _model.Cuota = value; }
        }

        public bool EsMensual
        {
            get { return _model.EsMensual; }
            set { _model.EsMensual = value; }
        }

        public bool EsAjeno
        {
            get { return _model.EsAjeno; }
            set { _model.EsAjeno = value; }
        }

        private IEnumerable<TagModel> _tags;
        public IEnumerable<TagModel> Tags
        {
            get { return _tags ?? (_tags = GetTags()); }
        }

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

        private bool SaveCommandCanExecute()
        {
            return true;
        }

        private void SaveCommandExecute()
        {
            _modelOriginal.DescripcionAdicional = _model.DescripcionAdicional;
            _modelOriginal.Monto = _model.Monto;
            _modelOriginal.EsMensual = _model.EsMensual;
            _modelOriginal.EsAjeno = _model.EsAjeno;
            _modelOriginal.Tags = this.Tags.Where(a => a.Seleccionada).Select(a => a.Tag).ToList();
            
            CloseCommand.Execute(null);
        }

        public event EventHandler<EventArgs> SaveEvent;
        private void OnSaveEvent()
        {
            var handler = this.SaveEvent;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        
    }
}
