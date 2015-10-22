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
            Cuota = string.Empty;
            Error = string.Empty;
        }

        public TipoMovimiento Tipo { get; set; }

        public int IdMovimiento
        {
            get;
            set;
        }

        public DateTime Fecha
        {
            get;
            set;
        }

        public string Descripcion
        {
            get;
            set;
        }

        public string DescripcionAdicional
        {
            get;
            set;
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

        public bool EsMensual { get; set; }

        public bool EsAjeno { get; set; }

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

        public bool IsSelected { get; set; }

        public int CoutasPendientes
        {
            get
            {
                var cuotasPendientes = 0;
                if (!Cuota.IsNullOrEmpty())
                {
                    var cuotasPagas = int.Parse(Cuota.Substring(0, 2));
                    var totalCuotas = int.Parse(Cuota.Substring(3, 2));

                    cuotasPendientes = totalCuotas - cuotasPagas;
                }

                return cuotasPendientes;
            }
        }

        public void IncrementarCuotasPendientes()
        {
            if (CoutasPendientes > 0)
            {
                var cuotasPagas = int.Parse(Cuota.Substring(0, 2));
                cuotasPagas++;

                Cuota = cuotasPagas.ToString("00") + Cuota.Substring(2, 3);
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            var str = string.Format("{0}, {1}", Descripcion, Monto);
            return str;
        }
    }
}
