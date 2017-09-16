using ampersand.Core;
using ampersand.Core.Common;
using ampersand_pb.Models;
using ampersand_pb.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ampersand_pb.ViewModels
{
    public class SeleccionDeMediosDePagoViewModel: BaseViewModel
    {
        public SeleccionDeMediosDePagoViewModel(ConfiguracionModel configuracionM)
        {
            _configuracionM = configuracionM;

            var mediosDePagoSeleccionados = Settings.Default.MediosDePagoSeleccionados
                .Split(';')
                .Where(a => !a.IsNullOrEmpty()).ToList();

            MediosDePago = configuracionM.MediosDePago.Clone();
            foreach (var medioDePago in MediosDePago)
            {
                medioDePago.Seleccionado = mediosDePagoSeleccionados.Any() ?
                    mediosDePagoSeleccionados.Contains(medioDePago.Id) :
                    true;
                medioDePago.PropertyChanged += MedioDePago_PropertyChanged;
            }
        }
        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                foreach (var medioDePago in MediosDePago)
                    medioDePago.PropertyChanged -= MedioDePago_PropertyChanged;

                MediosDePago = null;
                _configuracionM = null;
            }
            base.Dispose(dispose);
        }

        private ConfiguracionModel _configuracionM;

        public IEnumerable<PagoModel> MediosDePago { get; private set; }

        public IEnumerable<string> GetIds()
        {
            return MediosDePago.Where(a => a.Seleccionado).Select(a => a.Id);
        }

        private void MedioDePago_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnSeleccionDeMediosDePagoCambiada();
            Settings.Default.MediosDePagoSeleccionados = string.Join(";", MediosDePago.Where(a => a.Seleccionado).Select(a => a.Id));
            Settings.Default.Save();
        }

        public event EventHandler<EventArgs> SeleccionDeMediosDePagoCambiada;
        private void OnSeleccionDeMediosDePagoCambiada()
        {
            var handler = this.SeleccionDeMediosDePagoCambiada;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
