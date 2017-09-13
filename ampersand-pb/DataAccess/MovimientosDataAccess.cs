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
        public MovimientosDataAccess(ConfiguracionModel configuracion)
        {
            _configuracion = configuracion;
        }

        private ConfiguracionModel _configuracion;

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
                var pago = GetPago(xmlPago, resumen.Id, resumen.Descripcion);
                if (pago != null)
                    resultado.Add(pago);
            }

            return resultado;
        }

        private BaseMovimiento GetPago(XElement xmlPago, string idResumen, string descriResumen)
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
                    IdResumen = idResumen,
                    DescripcionResumen = descriResumen,
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

            if (Directory.Exists(_configuracion.CarpetaDeResumenes))
            {
                var files = Directory.GetFiles(_configuracion.CarpetaDeResumenes, "r*.xml");
                foreach (var file in files.OrderByDescending(a => a))
                {
                    var resumen = ResumenModel.GetFromFile(file);
                    if (resumen != null)
                    {
                        CargarResumen(resumen);
                        resultado.Add(resumen);
                    }
                }
            }

            return resultado;
        }

        private void CargarResumen(ResumenModel resumen)
        {
            var xdoc = XDocument.Load(resumen.FilePath);
            resumen.XDoc = xdoc;
            resumen.Tipo = GetTipo(GetValueFromXml<string>("Tipo", xdoc.Root, ""));

            resumen.Descripcion = GetValueFromXml<string>("Descripcion", xdoc.Root, "");
            resumen.Descripcion = _configuracion.MediosDePago.FirstOrDefault(a => a.Id == resumen.Id)?.Descripcion;

            resumen.Total = GetValueFromXml<decimal>("Total", xdoc.Root, 0M);

            var strFechaDeCierre = GetValueFromXml<string>("FechaDeCierre", xdoc.Root, "");
            var strProximoCierre = GetValueFromXml<string>("ProximoCierre", xdoc.Root, "");

            resumen.FechaDeCierre = strFechaDeCierre.ToDateTime();
            resumen.ProximoCierre = strProximoCierre.ToDateTime();
        }

        private TipoMovimiento GetTipo(string strTipo)
        {
            switch (strTipo)
            {
                case "Credito":
                    return TipoMovimiento.Credito;
                case "Debito":
                    return TipoMovimiento.Debito;
                default:
                    return TipoMovimiento.Efectivo;
            }
        }

        private IEnumerable<ResumenModel> GetResumenes(string periodo)
        {
            var files = Directory.GetFiles(_configuracion.CarpetaDeResumenes, "r*.xml");

            var resumenes = new List<ResumenModel>();

            var archivosDelPeriodo = files.Where(a => Path.GetFileName(a).Substring(2, 6).Equals(periodo)).ToList();
            foreach (var file in archivosDelPeriodo)
            {
                var resumen = ResumenModel.GetFromFile(file);
                if (resumen != null)
                {
                    CargarResumen(resumen);
                    resumenes.Add(resumen);
                }
            }

            return resumenes;
        }

        public ResumenAgrupadoModel GetUltimoResumen()
        {
            ResumenAgrupadoModel resultado = null;
            if (Directory.Exists(_configuracion.CarpetaDeResumenes))
            {
                var files = Directory.GetFiles(_configuracion.CarpetaDeResumenes, "r*.xml");
                var ultPeriodo = files.Select(a => Path.GetFileName(a).Substring(2, 6)).OrderBy(a => a).LastOrDefault();
                if (!ultPeriodo.IsNullOrEmpty())
                {
                    var resumenes = GetResumenes(ultPeriodo);

                    resultado = resumenes.Agrupar().FirstOrDefault();
                }
            }

            return resultado;
        }

        public ResumenAgrupadoModel GetResumen(string periodo)
        {
            ResumenAgrupadoModel resultado = null;
            if (Directory.Exists(_configuracion.CarpetaDeResumenes))
            {
                var resumenes = GetResumenes(periodo);

                resultado = resumenes.Agrupar().FirstOrDefault();
            }

            return resultado;
        }

        public void SaveMovimientos(ResumenAgrupadoModel resumenAgrupadoM, IEnumerable<BaseMovimiento> movimientos)
        {
            foreach (var resumenM in resumenAgrupadoM.Resumenes.Where(a => a.HuboCambios))
            {
                var movimientosDelResumen = movimientos.Where(a => a.IdResumen.Equals(resumenM.Id)).ToList();

                var total = movimientosDelResumen.Sum(a => a.Monto);

                var strProximoCierre = resumenM.ProximoCierre != DateTime.MinValue ?
                    resumenM.ProximoCierre.ToString("yyyyMMdd") :
                    string.Empty;

                var xdoc = new XDocument(new XElement("Movimientos", new XAttribute("Periodo", resumenM.Periodo),
                                                                     new XAttribute("FechaDeCierre", resumenM.FechaDeCierre.ToString("yyyyMMdd")),
                                                                     new XAttribute("Total", total),
                                                                     new XAttribute("Tipo", TipoMovimiento.Credito),
                                                                     new XAttribute("Descripcion", resumenM.Descripcion),
                                                                     new XAttribute("ProximoCierre", strProximoCierre)));

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
        ResumenAgrupadoModel GetUltimoResumen();
        IEnumerable<BaseMovimiento> GetMovimientos(ResumenAgrupadoModel resumenAgrupadoM);
        IEnumerable<BaseMovimiento> GetMovimientos(ResumenModel resumen);
        void SaveMovimientos(ResumenAgrupadoModel resumenAgrupadoM, IEnumerable<BaseMovimiento> movimientos);
        ResumenAgrupadoModel GetResumen(string periodo);
    }
}
