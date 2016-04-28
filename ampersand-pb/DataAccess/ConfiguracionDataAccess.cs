using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace ampersand_pb.DataAccess
{
    public class ConfiguracionDataAccess : IConfiguracionDataAccess
    {
        public ConfiguracionDataAccess()
        {
            _machineName = Environment.MachineName;
        }

        private string _machineName;
        private string fileName = "config.xml";

        public string GetFilesPath()
        {
            var filesPath = string.Empty;

            var appDirectory = Path.GetDirectoryName(App.ResourceAssembly.Location);
            var path = string.Format("{0}\\{1}", appDirectory, fileName);
            if (File.Exists(path))
            {
                try
                {
                    var xdoc = XDocument.Load(path);
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
            }
            else
            {

                var xdoc = new XDocument(new XElement("Configuraciones",
                    new XElement("PC", new XAttribute("Nombre", _machineName), new XElement("FilesPath", @"C:\Users\fabricio\Google Drive\Resumen y comprobantes\Apb\"))));
                xdoc.Save(path);
            }

            return filesPath;
        }
    }

    public interface IConfiguracionDataAccess
    {
        string GetFilesPath();
    }
}
