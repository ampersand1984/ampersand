﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Windows;
using ampersand_pb.Models;
using ampersand.Core.Common;
using System.Globalization;

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

        public IEnumerable<BaseMovimiento> GetMovimientos(string file, string periodo)
        {
            var resultado = new List<BaseMovimiento>();

            try
            {
                var xdoc = GetXDocument(file, periodo);
                var pagos = from XElement mov in xdoc.Root.Elements("Mov")
                            select mov;

                foreach (var xmlPago in pagos)
                {
                    var pago = GetPago(xmlPago);
                    if (pago != null)
                        resultado.Add(pago);
                }
            }
            catch (Exception)
            {

            }

            return resultado;
        }

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
                        resultado.Add(resumen);
                }
            }

            if (resultado.Any())
                resultado.First().EsElUtimoMes = true;

            return resultado;
        }
    }

    public interface IMovimientosDataAccess
    {
        IEnumerable<ResumenModel> GetResumenes();
        IEnumerable<BaseMovimiento> GetMovimientos(string file, string periodo);
    }
}
