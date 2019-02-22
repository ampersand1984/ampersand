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
            catch (Exception) { }

            return resultado;
        }

        public IEnumerable<BaseMovimiento> GetMovimientos(ResumenModel resumen)
        {
            var resultado = new List<BaseMovimiento>();

            var xmlPagos = from XElement mov in resumen.XDoc.Root.Elements("Mov")
                           .Union(resumen.XDoc.Root.Elements("Deuda"))
                           .Union(resumen.XDoc.Root.Elements("Saldo"))
                           select mov;

            var cotizacion = GetDecimal("Cotizacion", resumen.XDoc.Root, "1.00");

            foreach (var xmlMov in xmlPagos)
            {
                var pago = GetMovimiento(xmlMov, resumen.Id, resumen.Descripcion, cotizacion);
                if (pago != null)
                {
                    pago.Tipo = resumen.Tipo;
                    resultado.Add(pago);
                }
            }

            return resultado;
        }

        private BaseMovimiento GetMovimiento(XElement xmlMov, string idResumen, string descriResumen, decimal cotizacion)
        {
            var verif = GetValueFromXml("Verif", xmlMov, false);
            var idMovimiento = GetValueFromXml("IdMovimiento", xmlMov, 0);
            var strFecha = GetValueFromXml("Fecha", xmlMov, DateTime.MinValue.ToString("dd/MM/yyyy"));
            var strFechaVencimiento = GetValueFromXml("FechaVencimiento", xmlMov, string.Empty);
            var descri = GetValueFromXml("Descripcion", xmlMov, "");
            var descriAdic = GetValueFromXml("DescripcionAdicional", xmlMov, "");

            var monto = GetDecimal("Monto", xmlMov);
            var montoME = GetDecimal("MontoME", xmlMov);

            try
            {
                BaseMovimiento mov = null;

                switch (xmlMov.Name.ToString())
                {
                    case "Mov":
                        {
                            var cuota = GetValueFromXml("Cuota", xmlMov, "");

                            var strTags = GetValueFromXml("Tags", xmlMov, "");
                            var tags = strTags.IsNullOrEmpty() ?
                                Enumerable.Empty<string>() :
                                strTags.Split(';').ToList();

                            var esMensual = GetValueFromXml("EsMensual", xmlMov, false);
                            var esAjeno = GetValueFromXml("EsAjeno", xmlMov, false);
                            mov = new GastoModel();
                            mov.Cuota = cuota;
                            mov.Tags = tags;
                            mov.EsMensual = esMensual;
                            mov.EsAjeno = esAjeno;
                        }
                        break;

                    case "Deuda":
                        mov = new DeudaModel();
                        break;

                    case "Saldo":
                        mov = new SaldoModel();
                        break;

                    default:
                        mov = null;
                        break;
                }

                mov.Seleccionado = verif;
                mov.IdMovimiento = idMovimiento;
                mov.IdResumen = idResumen;
                mov.Fecha = strFecha.ToDateTime();
                mov.FechaVencimiento = strFechaVencimiento.IsNullOrEmpty() ? DateTime.MinValue : strFechaVencimiento.ToDateTime();
                mov.DescripcionResumen = descriResumen;
                mov.Descripcion = descri;
                mov.DescripcionAdicional = descriAdic;

                if (montoME != 0)
                    mov.SetMontoME(montoME, cotizacion);
                else
                    mov.SetMonto(monto);

                return mov;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private decimal GetDecimal(string property, XElement xmlPago, string strDefecto = "0.00")
        {
            var strDec = GetValueFromXml(property, xmlPago, strDefecto);
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
            var dec = decimal.Parse(strDec, numberFormatInfo);

            return dec;
        }

        public static T GetValueFromXml<T>(string attribute, XElement xml, T defaultValue)
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
            resumen.Tipo = GetTipo(GetValueFromXml("Tipo", xdoc.Root, ""));

            resumen.Descripcion = GetValueFromXml("Descripcion", xdoc.Root, "");
            resumen.Descripcion = _configuracion.MediosDePago.FirstOrDefault(a => a.Id == resumen.Id)?.Descripcion;

            resumen.Cotizacion = GetDecimal("Cotizacion", xdoc.Root, "1.00");

            var strFechaDeCierre = GetValueFromXml("FechaDeCierre", xdoc.Root, "");
            var strProximoCierre = GetValueFromXml("ProximoCierre", xdoc.Root, "");
            try
            {
                resumen.FechaDeCierre = strFechaDeCierre.ToDateTime();
                resumen.ProximoCierre = strProximoCierre.ToDateTime();
            }
            catch (Exception) { }
        }

        private TiposDeMovimiento GetTipo(string strTipo)
        {
            switch (strTipo)
            {
                case "Credito":
                    return TiposDeMovimiento.Credito;
                case "Debito":
                    return TiposDeMovimiento.Debito;
                default:
                    return TiposDeMovimiento.Efectivo;
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

                var strProximoCierre = resumenM.ProximoCierre != DateTime.MinValue ?
                    resumenM.ProximoCierre.ToString("yyyyMMdd") :
                    string.Empty;

                var movimientosElement = new XElement("Movimientos", new XAttribute("Periodo", resumenM.Periodo),
                                                                     new XAttribute("Tipo", TiposDeMovimiento.Credito),
                                                                     new XAttribute("Descripcion", resumenM.Descripcion),
                                                                     new XAttribute("Cotizacion", resumenM.Cotizacion));

                if (resumenM.Tipo == TiposDeMovimiento.Credito)
                {
                    movimientosElement.Add(new XAttribute("FechaDeCierre", resumenM.FechaDeCierre.ToString("yyyyMMdd")));
                    movimientosElement.Add(new XAttribute("ProximoCierre", strProximoCierre));
                }

                var xdoc = new XDocument(movimientosElement);

                foreach (var mov in movimientosDelResumen)
                {
                    var strMov = "Mov";

                    if (mov is DeudaModel)
                        strMov = "Deuda";

                    if (mov is SaldoModel)
                        strMov = "Saldo";

                    var xMov = new XElement(strMov, new XAttribute("Verif", mov.Seleccionado),
                                                    new XAttribute("IdMovimiento", mov.IdMovimiento),
                                                    new XAttribute("Fecha", mov.Fecha.ToString("yyyyMMdd")),
                                                    new XAttribute("Descripcion", mov.Descripcion));

                    if (mov.EsMonedaExtranjera)
                        xMov.Add(new XAttribute("MontoME", mov.MontoME));
                    else
                        xMov.Add(new XAttribute("Monto", mov.Monto));

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

                    if (mov.FechaVencimiento != DateTime.MinValue)
                        xMov.Add(new XAttribute("FechaVencimiento", mov.FechaVencimiento.ToString("yyyyMMdd")));

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
