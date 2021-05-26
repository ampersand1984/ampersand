using System;
using ampersand.Core;

namespace ampersand_pb.Models
{
    public class PagoModel : BaseModel, ICloneable, IEquatable<PagoModel>
    {
        public PagoModel()
        {
            Tipo = TiposDeMovimiento.Credito;
        }

        public TiposDeMovimiento Tipo { get; set; }

        private string _color;
        public string Color
        {
            get { return _color ?? string.Empty; }
            set { _color = value; }
        }

        private string _id;
        public string Id
        {
            get { return _id ?? string.Empty; }
            set { _id = value; }
        }

        private string _descripcion;
        public string Descripcion
        {
            get { return _descripcion ?? string.Empty; }
            set { _descripcion = value; }
        }

        private bool _seleccionado = true;
        public bool Seleccionado
        {
            get
            {
                return _seleccionado;
            }
            set
            {
                _seleccionado = value;
                OnPropertyChanged("Seleccionado");
            }
        }

        private bool _ocultar;
        public bool Ocultar
        {
            get { return _ocultar; }
            set { _ocultar = value; OnPropertyChanged("Ocultar"); }
        }

        public string EsExtensionDe { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone() as PagoModel;
        }

        public bool Equals(PagoModel other)
        {
            return other != null &&
                   Color == other.Color &&
                   Id == other.Id &&
                   Descripcion == other.Descripcion &&
                   Seleccionado == other.Seleccionado;
        }

        public override string ToString()
        {
            var str = string.Format("Id={0};Descri={1}", Id, Descripcion);

            return str;
        }
    }
}
