using ampersand.Core.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace ampersand_pb.Models
{
    public enum ThemesColors { Light, Dark }

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

        private IEnumerable<TagModel> _tags;
        public IEnumerable<TagModel> Tags
        {
            get
            {
                return _tags ?? (_tags= Enumerable.Empty<TagModel>());
            }
            set
            {
                _tags = value;
            }
        }

        public override bool Equals(object obj)
        {
            var configuracionM = obj as ConfiguracionModel;

            var equals = CarpetaDeResumenes.Equals(configuracionM.CarpetaDeResumenes);
            equals &= MediosDePago.AreEquals(configuracionM.MediosDePago);

            return equals;
        }

        public ConfiguracionModel Clone()
        {
            var clone = this.MemberwiseClone() as ConfiguracionModel;

            clone.MediosDePago = new List<PagoModel>(this.MediosDePago.Clone());

            return clone;
        }

        public void CopyValues(ConfiguracionModel configuracionM)
        {
            this.CarpetaDeResumenes = configuracionM.CarpetaDeResumenes;
            this.MediosDePago = new List<PagoModel>(configuracionM.MediosDePago.Clone());
        }
    }
}
