using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ampersand_pb.Models;

namespace ampersand_pb.DataAccess
{
    public class ConfiguracionDataAccess : IConfiguracionDataAccess
    {
        public ConfiguracionDataAccess()
        {
            _machineName = Environment.MachineName;
        }

        private string _machineName;
        private const string CONFIG_FILE_NAME = "config.xml";

        private string GetConfigFilePath()
        {
            var appDirectory = Path.GetDirectoryName(App.ResourceAssembly.Location);
            var filePath = string.Format("{0}\\{1}", appDirectory, CONFIG_FILE_NAME);
            return filePath;
        }

        private XDocument GetConfig()
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

        public string GetFilesPath()
        {
            var filesPath = string.Empty;

            try
            {
                var xdoc = GetConfig();
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

        public void SaveFilesPath(string filesPath)
        {
            try
            {
                var xdoc = GetConfig();
                foreach (var xelement in xdoc.Root.Elements("PC"))
                {
                    if (xelement.Attribute("Nombre").Value.Equals(_machineName))
                    {
                        xelement.Element("FilesPath").Value = filesPath;

                        string configFilePath = GetConfigFilePath();
                        xdoc.Save(configFilePath);

                        break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ConfiguracionModel GetConfiguracion(string carpetaDeResumenes)
        {
            var result = new ConfiguracionModel()
            {
                CarpetaDeResumenes = carpetaDeResumenes
            };
            //try
            //{
            //    var searchPattern = "r*.xml";
            //    var files = Directory.GetFiles(carpetaDeResumenes, searchPattern);
            //    var resumenesEnDisco = files.Select(a => a.Substring(0, 2)).Distinct().OrderBy(a => a);

            //    foreach (var item in resumenesEnDisco)
            //    {
            //        result.Resumenes.Add(new ResumenConfiguracionModel())
            //    }
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            
            return result;
        }
    }

    public interface IConfiguracionDataAccess
    {
        ConfiguracionModel GetConfiguracion(string carpetaDeResumenes);
        string GetFilesPath();
        void SaveFilesPath(string filesPath);
    }
}
