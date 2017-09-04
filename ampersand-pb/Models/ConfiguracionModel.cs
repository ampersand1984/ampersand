using ampersand.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ampersand_pb.Models
{
    public class ConfiguracionModel
    {
        private string _carpetaDeResumenes;
        public string CarpetaDeResumenes
        {
            get { return _carpetaDeResumenes ?? string.Empty; }
            set { _carpetaDeResumenes = value; }
        }


        public bool CarpetaDeResumenesValida
        {
            get
            {
                return Directory.Exists(CarpetaDeResumenes);
            }
        }

        private IEnumerable<PagoModel> _mediosDePago;
        public IEnumerable<PagoModel> MediosDePago
        {
            get
            {
                return _mediosDePago ?? Enumerable.Empty<PagoModel>();
            }
            set
            {
                _mediosDePago = value;
            }
        }

        public override bool Equals(object obj)
        {
            var configuracionM = obj as ConfiguracionModel;

            var equals = CarpetaDeResumenes.Equals(configuracionM.CarpetaDeResumenes);
            equals = MediosDePago.AreEquals(configuracionM.MediosDePago);

            return equals;
        }

        public ConfiguracionModel Clone()
        {
            var clone = this.MemberwiseClone() as ConfiguracionModel;

            clone.MediosDePago = new List<PagoModel>(this.MediosDePago.Select(a => a.Clone()));

            return clone;
        }

        public void CopyValues(ConfiguracionModel configuracionM)
        {
            this.CarpetaDeResumenes = configuracionM.CarpetaDeResumenes;
            this.MediosDePago = new List<PagoModel>(configuracionM.MediosDePago.Select(a => a.Clone()));
        }
    }
}
