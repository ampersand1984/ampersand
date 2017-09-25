﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ampersand_pb.Models;
using System.Collections.Generic;

namespace ampersand_pb.DataAccess
{
    public class ConfiguracionDataAccess : IConfiguracionDataAccess
    {
        public ConfiguracionDataAccess()
        {
            _machineName = Environment.MachineName;
        }

        private string _machineName;
        private const string CONFIGIGURACION_FILE_NAME = "config.xml";
        private const string CONFIGIGURACION_DE_PAGOS_FILE_NAME = "configuracionDePagos.xml";

        private string GetConfigFilePath()
        {
            var appDirectory = Path.GetDirectoryName(App.ResourceAssembly.Location);
            var filePath = string.Format("{0}\\{1}", appDirectory, CONFIGIGURACION_FILE_NAME);
            return filePath;
        }

        private XDocument GetConfiguracionXml()
        {
            XDocument xdoc = new XDocument(new XElement("Configuraciones",
                    new XElement("PC", new XAttribute("Nombre", _machineName), new XElement("FilesPath", "")))); ;

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
                    Tipo = GetTipoDeMovimiento(xelement.Attribute("Tipo").Value)
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
                xdoc.Root.Add(new XElement("MedioDePago", new XAttribute("Tipo", item.Tipo), new XAttribute("Id", item.Id), new XAttribute("Descripcion", item.Descripcion)));

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

            return filesPath;
        }

        public void GuardarConfiguracion(ConfiguracionModel configuracionM)
        {
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
                        var config = configuracionDePagos.FirstOrDefault(a => a.Id == id);

                        var medioDePago = new PagoModel() { Id = id, Descripcion = config?.Descripcion };

                        mediosDePago.Add(medioDePago);
                    }

                    result.MediosDePago = mediosDePago;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }
    }

    public interface IConfiguracionDataAccess
    {
        ConfiguracionModel GetConfiguracion();
        void GuardarConfiguracion(ConfiguracionModel configuracionM);
    }
}
