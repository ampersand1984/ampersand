using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ampersand.Core;
using ampersand.Core.Common;

namespace ampersand_pb.Models
{
    public enum TipoMovimiento { Efectivo, Debito, Credito }

    public class BaseMovimiento: BaseModel, ICloneable
    {
        public BaseMovimiento()
        {
            Tipo = TipoMovimiento.Credito;
        }

        public TipoMovimiento Tipo { get; set; }

        private string _tipoDescripcion;
        public string TipoDescripcion
        {
            get
            {
                return _tipoDescripcion ?? string.Empty;
            }

            set
            {
                _tipoDescripcion = value;
            }
        }

        private int _idMovimiento;
        public int IdMovimiento
        {
            get { return _idMovimiento; }
            set { _idMovimiento = value; }
        }

        public DateTime Fecha
        {
            get;
            set;
        }

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

        public decimal Monto
        {
            get;
            set;
        }

        public string Cuota
        {
            get;
            set;
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
            clone.Tags = new List<string>(this.Tags);
            return clone;
        }

        public override string ToString()
        {
            var str = string.Format("{0}, {1}", Descripcion, Monto);
            return str;
        }

        public event EventHandler<IsSelectedChangedEventHandler> IsSelectedChangedEvent;
        private void OnIsSelectedChangedEvent()
        {
            var handler = this.IsSelectedChangedEvent;
            if (handler != null)
                handler(this, new IsSelectedChangedEventHandler(this.IsSelected));
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
