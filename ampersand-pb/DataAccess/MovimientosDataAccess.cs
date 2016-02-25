using ampersand.Core.Common;
using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace ampersand_pb.DataAccess
{
    public class MovimientosDataAccess : IMovimientosDataAccess
    {
        public MovimientosDataAccess(string path)
        {
            _path = path;
        }

        private string _path = string.Empty;

        private XDocument GetXDocument(string file, string periodo)
        {
            XDocument xdoc = null;

            try
            {
                xdoc = XDocument.Load(file);
            }
            catch (FileNotFoundException)
            {
                xdoc = new XDocument(new XElement("Movimientos", new XAttribute("Periodo", periodo)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return xdoc;
        }

        //private XDocument GetNewDocument(string fecha)
        //{
        //    var xdoc = new XDocument(new XElement("Movimientos", new XAttribute("Periodo", fecha)));
        //    xdoc.Save(Path.GetDirectoryName(App.ResourceAssembly.Location) + @"\" + fecha + ".xml");
        //    return xdoc;
        //}

        public IEnumerable<BaseMovimiento> GetMovimientos(ResumenAgrupadoModel resumenAgrupadoM)
        {
            var resultado = new List<BaseMovimiento>();

            try
            {
                foreach (var resumen in resumenAgrupadoM.Resumenes)
                {
                    var pagos = GetMovimientos(resumen);
                    resultado.AddRange(pagos);
                }
            }
            catch (Exception)
            {

            }

            return resultado;
        }

        public IEnumerable<BaseMovimiento> GetMovimientos(ResumenModel resumen)
        {
            var resultado = new List<BaseMovimiento>();

            var xmlPagos = from XElement mov in resumen.XDoc.Root.Elements("Mov")
                        select mov;

            foreach (var xmlPago in xmlPagos)
            {
                var pago = GetPago(xmlPago);
                if (pago != null)
                {
                    pago.TipoDescripcion = resumen.Descripcion;
                    resultado.Add(pago);
                }
            }

            return resultado;
        }

        //public IEnumerable<BaseMovimiento> GetMovimientosDeResumenAnterior(string periodoActual)
        //{
        //    var resumenes = this.GetResumenes();

        //    var periodoAnterior = (periodoActual + "01").ToDateTime().AddMonths(-1).GetPeriodo();

        //    var resumen = resumenes.FirstOrDefault(a => a.Periodo.Equals(periodoAnterior));
        //    if (resumen != null)
        //    {
        //        var movimientos = this.GetMovimientos(resumen.FilePath, periodoAnterior);
        //        return movimientos;
        //    }
        //    else
        //    {
        //        throw new ArgumentException(string.Format("No hay información del período {0}", periodoAnterior));
        //    }
        //}

        private BaseMovimiento GetPago(XElement xmlPago)
        {
            var idMovimiento = GetValueFromXml<int>("IdMovimiento", xmlPago, 0);
            var strFecha = GetValueFromXml<string>("Fecha", xmlPago, DateTime.MinValue.ToString("dd/MM/yyyy"));
            var descri = GetValueFromXml<string>("Descripcion", xmlPago, "");
            var descriAdic = GetValueFromXml<string>("DescripcionAdicional", xmlPago, "");

            var strMonto = GetValueFromXml<string>("Monto", xmlPago, "0.00");
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
            var monto = decimal.Parse(strMonto, numberFormatInfo);

            var cuota = GetValueFromXml<string>("Cuota", xmlPago, "");

            var strTags = GetValueFromXml<string>("Tags", xmlPago, "");
            var tags = strTags.IsNullOrEmpty() ?
                Enumerable.Empty<string>() :
                strTags.Split(';').ToList();
            

            var esMensual = GetValueFromXml<bool>("EsMensual", xmlPago, false);
            var esAjeno = GetValueFromXml<bool>("EsAjeno", xmlPago, false);

            try 
	        {
                var pago = new BaseMovimiento
                {
                    IdMovimiento = idMovimiento,
                    Fecha = strFecha.ToDateTime(),
                    Descripcion = descri,
                    DescripcionAdicional = descriAdic,
                    Monto = monto,
                    Cuota = cuota,
                    Tags = tags,
                    EsMensual = esMensual,
                    EsAjeno = esAjeno
                };
                return pago;
	        }
	        catch (Exception)
	        {
                return null;
	        }
        }

        private T GetValueFromXml<T>(string attribute, XElement xml, T defaultValue)
        {
            var value = xml.Attribute(attribute);
            if (value != null)
            {
                try
                {
                    return (T)Convert.ChangeType(value.Value, typeof(T));
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public IEnumerable<ResumenModel> GetResumenes()
        {
            var resultado = new List<ResumenModel>();

            if (Directory.Exists(_path))
            {
                var files = Directory.GetFiles(_path, "r*.xml");
                foreach (var file in files.OrderByDescending(a => a))
                {
                    var resumen = ResumenModel.GetFromFile(file);

                    if (resumen != null)
                    {
                        var xdoc = XDocument.Load(file);
                        resumen.XDoc = xdoc;
                        resumen.Tipo = TipoMovimiento.Credito;
                        resumen.Descripcion = GetValueFromXml<string>("Descripcion", xdoc.Root, "");
                        resumen.Total = GetValueFromXml<decimal>("Total", xdoc.Root, 0M);
                        resultado.Add(resumen);
                    }
                }
            }

            return resultado;
        }

        public void SaveMovimientos(ResumenAgrupadoModel resumenAgrupadoM, IEnumerable<BaseMovimiento> movimientos)
        {
            foreach (var resumenM in resumenAgrupadoM.Resumenes.Where(a => a.HuboCambios))
            {
                var movimientosDelResumen = movimientos.Where(a => a.TipoDescripcion.Equals(resumenM.Descripcion)).ToList();

                var total = movimientosDelResumen.Sum(a => a.Monto);

                var xdoc = new XDocument(new XElement("Movimientos", new XAttribute("Periodo", resumenM.Periodo),
                                                                     new XAttribute("FechaDeCierre", resumenM.FechaDeCierre.ToString("yyyyMMdd")),
                                                                     new XAttribute("Total", total),
                                                                     new XAttribute("Tipo", TipoMovimiento.Credito),
                                                                     new XAttribute("Descripcion", resumenM.Descripcion)));

                foreach (var mov in movimientosDelResumen)
                {
                    var xMov = new XElement("Mov", new XAttribute("IdMovimiento", mov.IdMovimiento),
                                                   new XAttribute("Fecha", mov.Fecha.ToString("yyyyMMdd")),
                                                   new XAttribute("Descripcion", mov.Descripcion),
                                                   new XAttribute("Monto", mov.Monto));

                    if (mov.DescripcionAdicional.Length > 0)
                        xMov.Add(new XAttribute("DescripcionAdicional", mov.DescripcionAdicional));

                    if (mov.Cuota.Length > 0)
                        xMov.Add(new XAttribute("Cuota", mov.Cuota));

                    if (mov.Tags.Any())
                        xMov.Add(new XAttribute("Tags", string.Join(";", mov.Tags)));

                    if (mov.EsMensual)
                        xMov.Add(new XAttribute("EsMensual", mov.EsMensual));

                    if (mov.EsAjeno)
                        xMov.Add(new XAttribute("EsAjeno", mov.EsAjeno));

                    xdoc.Root.Add(xMov);
                }

                xdoc.Save(resumenM.FilePath);
            }
        }
    }

    public interface IMovimientosDataAccess
    {
        IEnumerable<ResumenModel> GetResumenes();
        IEnumerable<BaseMovimiento> GetMovimientos(ResumenAgrupadoModel resumenAgrupadoM);
        IEnumerable<BaseMovimiento> GetMovimientos(ResumenModel resumen);

        //IEnumerable<BaseMovimiento> GetMovimientosDeResumenAnterior(string periodoActual);

        void SaveMovimientos(ResumenAgrupadoModel resumenAgrupadoM, IEnumerable<BaseMovimiento> movimientos);
    }
}
