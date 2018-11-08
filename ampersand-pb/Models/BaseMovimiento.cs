using ampersand.Core;
using ampersand.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ampersand_pb.Models
{
    public enum TiposDeMovimiento { Efectivo, Debito, Credito, Deuda }

    public abstract class BaseMovimiento: BaseModel, ICloneable
    {
        protected BaseMovimiento()
        {
            Fecha = DateTime.Today;
        }

        public TiposDeMovimiento Tipo { get; set; }

        private string _descripcionResumen;
        public string DescripcionResumen
        {
            get
            {
                return _descripcionResumen ?? string.Empty;
            }

            set
            {
                _descripcionResumen = value;
            }
        }

        private bool _seleccionado;
        public bool Seleccionado
        {
            get { return _seleccionado; }
            set { _seleccionado = value; OnPropertyChanged("Seleccionado"); }
        }
        
        public string IdResumen { get; set; }
        
        public int IdMovimiento { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Today;

        private string _descripcion;
        public string Descripcion
        {
            get { return _descripcion ?? string.Empty; }
            set { _descripcion = value; }
        }

        private string _descripcionAdicional;
        public string DescripcionAdicional
        {
            get { return _descripcionAdicional ?? string.Empty; }
            set { _descripcionAdicional = value; OnPropertyChanged("DescripcionAdicional"); }
        }

        private decimal _monto;
        public decimal Monto
        {
            get
            {
                return _monto;
            }
        }

        private decimal _montoME;
        public decimal MontoME
        {
            get
            {
                return _montoME;
            }
            set
            {
                _montoME = value; 
                _monto = value * Cotizacion;
                RefrescarMontos();
            }
        }

        private decimal _cotizacion = 1;
        public decimal Cotizacion
        {
            get
            {
                return _cotizacion;
            }
        }

        public bool EsMonedaExtranjera
        {
            get
            {
                return Cotizacion != 1;
            }
        }

        private string _cuota;
        public string Cuota
        {
            get
            {
                return _cuota ?? string.Empty;
            }
            set
            {
                _cuota = value;
                OnPropertyChanged("Cuota");
            }
        }

        private bool _esMensual;
        public bool EsMensual
        {
            get { return _esMensual; }
            set { _esMensual = value; OnPropertyChanged("EsMensual"); }
        }

        private bool _esAjeno;
        public bool EsAjeno
        {
            get { return _esAjeno; }
            set { _esAjeno = value; OnPropertyChanged("EsAjeno"); }
        }
        
        public string Error
        {
            get;
            set;
        }

        private IEnumerable<string> _tags = Enumerable.Empty<string>();
        public IEnumerable<string> Tags
        {
            get { return _tags; }
            set { _tags = value; OnPropertyChanged("Tags"); }
        }

        public int CoutasPendientes
        {
            get
            {
                var cuotasPendientes = -1;
                if (!Cuota.IsNullOrEmpty())
                {
                    var slashIndex = Cuota.IndexOf("/");

                    var cuotasPagas = int.Parse(Cuota.Substring(0, slashIndex));
                    var totalCuotas = int.Parse(Cuota.Substring(slashIndex + 1));

                    cuotasPendientes = totalCuotas - cuotasPagas;
                }

                return cuotasPendientes;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
                OnIsSelectedChangedEvent();
            }
        }

        public void IncrementarCuotasPendientes()
        {
            if (CoutasPendientes > 0)
            {
                var slashIndex = Cuota.IndexOf("/");

                var cuotasPagas = int.Parse(Cuota.Substring(0, slashIndex));
                cuotasPagas++;

                Cuota = cuotasPagas.ToString("00") + Cuota.Substring(slashIndex);
            }
        }

        public object Clone()
        {
            var clone = this.MemberwiseClone() as BaseMovimiento;
            clone.Tags = Tags.Clone();
            return clone;
        }

        public override string ToString()
        {
            var str = string.Format("{0}, {1}, {2}, {3}", IdResumen, Fecha.ToString("dd/MM/yyyy"), Descripcion, Monto);
            return str;
        }

        public event EventHandler<IsSelectedChangedEventHandler> IsSelectedChangedEvent;
        private void OnIsSelectedChangedEvent()
        {
            this.IsSelectedChangedEvent?.Invoke(this, new IsSelectedChangedEventHandler(this.IsSelected));
        }

        public void SetMonto(decimal monto)
        {
            _monto = monto;
            _montoME = monto / Cotizacion;
            RefrescarMontos();
        }

        public void SetMontoME(decimal montoME, decimal cotizacion)
        {
            _cotizacion = cotizacion > 0 ? cotizacion : 1;

            _montoME = montoME;
            _monto = montoME * Cotizacion;
            _monto = Math.Round(_monto, 2);
            RefrescarMontos();
        }

        public void RefrescarPropiedades()
        {
            OnPropertyChanged("Descripcion");
            OnPropertyChanged("DescripcionAdicional");
            OnPropertyChanged("Fecha");
            OnPropertyChanged("Tipo");
            OnPropertyChanged("DescripcionResumen");
            OnPropertyChanged("Cuota");
            OnPropertyChanged("EsMensual");
            OnPropertyChanged("EsAjeno");
            OnPropertyChanged("Tags");
            RefrescarMontos();
        }

        public void RefrescarMontos()
        {
            OnPropertyChanged("Monto");
            OnPropertyChanged("MontoME");
            OnPropertyChanged("EsMonedaExtranjera");
            OnPropertyChanged("Cotizacion");
        }

        internal void CopyValues(BaseMovimiento model)
        {
            IdResumen = model.IdResumen;
            DescripcionResumen = model.DescripcionResumen;

            IdMovimiento = model.IdMovimiento;
            Descripcion = model.Descripcion;
            DescripcionAdicional = model.DescripcionAdicional;

            Fecha = model.Fecha;
            Tipo = model.Tipo;
            DescripcionResumen = model.DescripcionResumen;
            Cuota = model.Cuota;
            _monto = model.Monto;
            _montoME = model.MontoME;
            _cotizacion = model.Cotizacion;
            EsMensual = model.EsMensual;
            EsAjeno = model.EsAjeno;
        }

        public override bool Equals(object obj)
        {
            var model = obj as BaseMovimiento;

            var equal = true;

            if (model != null)
            {
                equal &= Tipo == model.Tipo;
                equal &= IdResumen == model.IdResumen;
                equal &= DescripcionResumen == model.DescripcionResumen;
                equal &= IdMovimiento == model.IdMovimiento;
                equal &= Fecha == model.Fecha;
                equal &= Descripcion == model.Descripcion;
                equal &= DescripcionAdicional == model.DescripcionAdicional;
                equal &= Monto == model.Monto;
                equal &= Cuota == model.Cuota;
                equal &= EsMensual == model.EsMensual;
                equal &= EsAjeno == model.EsAjeno;
                equal &= string.Join(";", Tags) == string.Join(";", model.Tags);
            }

            return equal;
        }
    }

    public class IsSelectedChangedEventHandler: EventArgs
    {
        public IsSelectedChangedEventHandler(bool isSelected)
        {
            IsSelected = isSelected;
        }

        public bool IsSelected { get; private set; }
    }
}
