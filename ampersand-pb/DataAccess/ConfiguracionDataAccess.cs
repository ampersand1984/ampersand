using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ampersand_pb.Models;
using Serilog;

namespace ampersand_pb.DataAccess
{
    public class ConfiguracionDataAccess : IConfiguracionDataAccess
    {
        public ConfiguracionDataAccess()
        {
            _machineName = Environment.MachineName;
            Log.Information($"ConfiguracionDataAccess start");
            Log.Information($"ConfiguracionDataAccess machineName: {_machineName}");
        }

        private readonly string _machineName;
        private const string CONFIGIGURACION_FILE_NAME = "config.xml";
        private const string CONFIGIGURACION_DE_PAGOS_FILE_NAME = "configuracionDePagos.xml";

        private string GetConfigFilePath()
        {
            var appDirectory = Path.GetDirectoryName(App.ResourceAssembly.Location);
            var filePath = string.Format("{0}\\{1}", appDirectory, CONFIGIGURACION_FILE_NAME);

            Log.Information($"ConfiguracionDataAccess GetConfigFilePath: {filePath}");
            return filePath;
        }

        private XDocument GetConfiguracionXml()
        {
            XDocument xdoc = new XDocument(new XElement("Configuraciones",
                    new XElement("PC", new XAttribute("Nombre", _machineName), new XElement("FilesPath", ""))));

            string configFilePath = GetConfigFilePath();
            if (File.Exists(configFilePath))
            {
                try
                {
                    xdoc = XDocument.Load(configFilePath);
                }
                catch (Exception) { }
            }
            else
            {
                xdoc.Save(configFilePath);
            }

            return xdoc;
        }

        private XDocument GetConfiguracionDePagosXml(string carpetaDeResumenes)
        {
            XDocument xdoc = new XDocument(new XElement("ConfiguracionDePagos"));

            var filePath = string.Format("{0}\\{1}", carpetaDeResumenes, CONFIGIGURACION_DE_PAGOS_FILE_NAME);

            if (File.Exists(filePath))
            {
                try
                {
                    xdoc = XDocument.Load(filePath);
                }
                catch (Exception) { }
            }
            else
            {
                xdoc.Save(filePath);
            }

            return xdoc;
        }

        private IEnumerable<PagoModel> GetConfiguracionDePagos(string carpetaDeResumenes)
        {
            var result = new List<PagoModel>();

            var xdoc = GetConfiguracionDePagosXml(carpetaDeResumenes);

            foreach (var xelement in xdoc.Root.Elements("MedioDePago"))
            {
                result.Add(new PagoModel()
                {
                    Id = xelement.Attribute("Id").Value,
                    Descripcion = xelement.Attribute("Descripcion").Value,
                    Tipo = GetTipoDeMovimiento(xelement.Attribute("Tipo").Value),
                    Ocultar = MovimientosDataAccess.GetValueFromXml("Ocultar", xelement, false),
                    EsExtensionDe = MovimientosDataAccess.GetValueFromXml("EsExtensionDe", xelement, "")
                });
            }

            return result;
        }

        private TiposDeMovimiento GetTipoDeMovimiento(string strTipo)
        {
            switch (strTipo)
            {
                case "Credito": return TiposDeMovimiento.Credito;
                case "Debito": return TiposDeMovimiento.Debito;
                default: return TiposDeMovimiento.Efectivo;
            }
        }

        private void GuardarConfiguracionDePagos(string carpetaDeResumenes, IEnumerable<PagoModel> mediosDePagos)
        {
            XDocument xdoc = new XDocument(new XElement("ConfiguracionDePagos"));
            foreach (var item in mediosDePagos)
                xdoc.Root.Add(new XElement("MedioDePago", new XAttribute("Tipo", item.Tipo),
                                                          new XAttribute("Id", item.Id),
                                                          new XAttribute("Descripcion", item.Descripcion),
                                                          new XAttribute("Ocultar", item.Ocultar),
                                                          new XAttribute("EsExtensionDe", item.EsExtensionDe ?? string.Empty)));

            var filePath = string.Format("{0}\\{1}", carpetaDeResumenes, CONFIGIGURACION_DE_PAGOS_FILE_NAME);
            xdoc.Save(filePath);
        }

        private string GetCarpetaDeResumenes()
        {
            var filesPath = string.Empty;

            try
            {
                var xdoc = GetConfiguracionXml();
                foreach (var xelement in xdoc.Root.Elements("PC"))
                {
                    if (xelement.Attribute("Nombre").Value.Equals(_machineName))
                    {
                        filesPath = xelement.Element("FilesPath").Value;
                        break;
                    }
                }
            }
            catch (Exception) { }

            Log.Information($"ConfiguracionDataAccess GetCarpetaDeResumenes: {filesPath}");
            return filesPath;
        }

        public void GuardarConfiguracion(ConfiguracionModel configuracionM)
        {
            Log.Information($"ConfiguracionDataAccess GuardarConfiguracion start");
            try
            {
                var xdoc = GetConfiguracionXml();
                foreach (var xelement in xdoc.Root.Elements("PC"))
                {
                    if (xelement.Attribute("Nombre").Value.Equals(_machineName))
                    {
                        xelement.Element("FilesPath").Value = configuracionM.CarpetaDeResumenes;

                        string configFilePath = GetConfigFilePath();
                        xdoc.Save(configFilePath);

                        break;
                    }
                }

                GuardarConfiguracionDePagos(configuracionM.CarpetaDeResumenes, configuracionM.MediosDePago);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ConfiguracionModel GetConfiguracion()
        {
            var result = new ConfiguracionModel()
            {
                CarpetaDeResumenes = GetCarpetaDeResumenes()
            };

            if (result.CarpetaDeResumenesValida)
            {
                try
                {
                    var configuracionDePagos = GetConfiguracionDePagos(result.CarpetaDeResumenes);

                    var searchPattern = "r*.xml";
                    var files = Directory.GetFiles(result.CarpetaDeResumenes, searchPattern);
                    var resumenesEnDisco = files.Select(a => Path.GetFileName(a).Substring(0, 2)).Distinct().OrderBy(a => a);

                    var mediosDePago = new List<PagoModel>();
                    foreach (var id in resumenesEnDisco)
                    {
                        var configDePago = configuracionDePagos.FirstOrDefault(a => a.Id == id);
                        if (configDePago != null)
                        {
                            var medioDePago = new PagoModel()
                            {
                                Id = id,
                                Descripcion = configDePago?.Descripcion,
                                Tipo = configDePago.Tipo,
                                Ocultar = configDePago.Ocultar,
                                EsExtensionDe = configDePago.EsExtensionDe ?? string.Empty
                            };

                            mediosDePago.Add(medioDePago);
                        }
                    }

                    result.MediosDePago = mediosDePago;

                    result.Tags = GetTags(result.CarpetaDeResumenes);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }

        private IEnumerable<TagModel> GetTags(string carpetaDeResumenes)
        {
            try
            {
                XDocument xdoc = XDocument.Load(carpetaDeResumenes + "\\tags.xml");

                var tags = xdoc.Root.Elements("Tag").Select(a => new TagModel() { Tag = a.Value });

                //var tags = new List<TagModel>()
                //{
                //    new TagModel() { Tag = "super" },
                //    new TagModel() { Tag = "chinos" },
                //    new TagModel() { Tag = "nafta" },
                //    new TagModel() { Tag = "ropa" },
                //    new TagModel() { Tag = "donado" },
                //    new TagModel() { Tag = "auto" },
                //    new TagModel() { Tag = "delivery" },
                //    new TagModel() { Tag = "salida" },
                //    new TagModel() { Tag = "farmacia" },
                //    new TagModel() { Tag = "vacaciones" }
                //};
                return tags;
            }
            catch (Exception)
            {
                return new List<TagModel>();
            }
        }
    }

    public interface IConfiguracionDataAccess
    {
        ConfiguracionModel GetConfiguracion();
        void GuardarConfiguracion(ConfiguracionModel configuracionM);
    }
}
