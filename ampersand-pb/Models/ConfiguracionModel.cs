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
                if (_tags == null)
                {
                    _tags = new List<TagModel>()
                    {
                        new TagModel() { Tag = "super" },
                        new TagModel() { Tag = "chinos" },
                        new TagModel() { Tag = "nafta" },
                        new TagModel() { Tag = "ropa" },
                        new TagModel() { Tag = "donado" },
                        new TagModel() { Tag = "auto" },
                        new TagModel() { Tag = "delivery" },
                        new TagModel() { Tag = "salida" },
                        new TagModel() { Tag = "farmacia" },
                        new TagModel() { Tag = "vacaciones" }
                    };
                }

                return _tags;
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
