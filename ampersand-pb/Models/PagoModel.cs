using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ampersand_pb.Models
{
    public class PagoModel
    {
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

        public override bool Equals(object obj)
        {
            var pagoM = obj as PagoModel;
            if (pagoM != null)
                return Id.Equals(pagoM.Id) && Descripcion.Equals(pagoM.Descripcion);

            return false;
        }

        public PagoModel Clone()
        {
            return this.MemberwiseClone() as PagoModel;
        }
    }
}
